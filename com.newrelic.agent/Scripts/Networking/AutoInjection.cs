#if (UNITY_EDITOR && !UNITY_WEBGL)
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEngine;

namespace NewRelic.Networking
{
    [InitializeOnLoad]
    public class AutoInjection
    {
        static AutoInjection()
        {
            List<string> assemblies = new List<string>();

            try
            {
                UnityEditor.Compilation.CompilationPipeline.compilationStarted += (o) =>
                {
                    EditorApplication.LockReloadAssemblies();
                    assemblies.Clear();
                };

                UnityEditor.Compilation.CompilationPipeline.assemblyCompilationFinished += (asmName, message) =>
                {
                    assemblies.Add(asmName);
                    Debug.LogFormat("asmName {0} .", asmName);

                };

                UnityEditor.Compilation.CompilationPipeline.compilationFinished += (o) =>
                {
                    foreach (var assembly in assemblies.Where(a => a.EndsWith("Assembly-CSharp.dll")))
                    {

                        Debug.LogFormat("Attempting to rewrite {0} .", assembly);
                        var assemblyDefinition = AssemblyDefinition.ReadAssembly(assembly, new ReaderParameters() { ReadWrite = true });
                        InjectIntoAssembly(assemblyDefinition);
                        Debug.LogFormat("Finished rewriting {0}", assemblyDefinition.FullName);


                    }
                    EditorApplication.UnlockReloadAssemblies();
                };



            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
            }
        }

        private static void InjectIntoAssembly(AssemblyDefinition assemblyDefinition)
        {
            try
            {
                var modifiedAssembly = false;
                var sendWebRequestWrapperMethodInfo = typeof(NRWrappedUnityWebRequest).GetMethod("SendWebRequest");
                var disposeWebrequestWrapperMethodInfo = typeof(NRWrappedUnityWebRequest).GetMethod("DisposeWebRequest");

                foreach (var moduleDefinition in assemblyDefinition.Modules)
                {
                    if (!moduleDefinition.HasAssemblyReferences || !moduleDefinition.AssemblyReferences.Any(ar => ar.Name.Contains("UnityWebRequestModule")))
                    {
                        continue;
                    }

                    Debug.LogFormat("In {0} with reference to UnityWebRequest. Attempting to process module {1}.", assemblyDefinition, moduleDefinition.Name);

                    var injectedType = moduleDefinition.Types.SingleOrDefault(t => t.IsNotPublic && t.Name == "__NRInjected__");
                    if (injectedType == null)
                    {
                        Debug.Log("Module not processed yet");

                        MethodReference sendWebRequestWrapperMethodReference = null;
                        MethodReference disposeWebRequestWrapperMethodReference = null;


                        foreach (var typeDefinition in GetAllTypesInModule(moduleDefinition).Where(t => t.Name != "NRWrappedUnityWebRequest"))
                        {
                            foreach (var methodDefinition in typeDefinition.Methods)
                            {

                                //Debug.LogFormat("Checking {0}.{1}", typeDefinition.Name, methodDefinition.Name);
                                if (methodDefinition.HasBody && methodDefinition.Body.Instructions.Any(i => i.OpCode.Code == Code.Callvirt && i.Operand is MethodReference))
                                {
                                    var sendWebRequestInstructionsToReplace = new List<Instruction>();
                                    var disposeWebRequestInstructionsToReplace = new List<Instruction>();

                                    foreach (var instruction in methodDefinition.Body.Instructions.Where(i => i.OpCode.Code == Code.Callvirt && i.Operand is MethodReference))
                                    {
                                        var instructionMethodReference = (MethodReference)instruction.Operand;
                                        if (instructionMethodReference.FullName == "UnityEngine.Networking.UnityWebRequestAsyncOperation UnityEngine.Networking.UnityWebRequest::SendWebRequest()")
                                        {
                                            Debug.LogFormat("Possibly instrument {0}.{1} call {2}", typeDefinition.Name, methodDefinition.Name, instructionMethodReference.FullName);
                                            sendWebRequestInstructionsToReplace.Add(instruction);
                                        }

                                        if (instructionMethodReference.FullName == "System.Void UnityEngine.Networking.UnityWebRequest::Dispose()" || instructionMethodReference.FullName == "System.Void System.IDisposable::Dispose()")
                                        {
                                            if (instruction.Previous.OpCode.Code != Code.Constrained)
                                            {
                                                Debug.LogFormat("Possibly instrument {0}.{1} call {2}", typeDefinition.Name, methodDefinition.Name, instructionMethodReference.FullName);
                                                disposeWebRequestInstructionsToReplace.Add(instruction);
                                            }
                                        }
                                    }

                                    if (sendWebRequestInstructionsToReplace.Any())
                                    {
                                        modifiedAssembly = true;

                                        if (sendWebRequestWrapperMethodReference == null)
                                        {
                                            sendWebRequestWrapperMethodReference = moduleDefinition.ImportReference(sendWebRequestWrapperMethodInfo);
                                        }

                                        Debug.Log("****** Found call to replace");
                                        var iLProcessor = methodDefinition.Body.GetILProcessor();

                                        foreach (var instructionToReplace in sendWebRequestInstructionsToReplace)
                                        {
                                            Debug.Log("******* Replacing UnityWebRequst::SendWebRequestCall with wrapper call. *******");
                                            var callWrapperInstruction = iLProcessor.Create(Mono.Cecil.Cil.OpCodes.Call, sendWebRequestWrapperMethodReference);
                                            iLProcessor.Replace(instructionToReplace, callWrapperInstruction);
                                        }

                                    }

                                    if (disposeWebRequestInstructionsToReplace.Any())
                                    {
                                        modifiedAssembly = true;
                                        if (disposeWebRequestWrapperMethodReference == null)
                                        {
                                            disposeWebRequestWrapperMethodReference = moduleDefinition.ImportReference(disposeWebrequestWrapperMethodInfo);
                                        }

                                        //Debug.Log("****** Found call to replace");
                                        var iLProcessor = methodDefinition.Body.GetILProcessor();

                                        foreach (var instructionToReplace in disposeWebRequestInstructionsToReplace)
                                        {
                                            Debug.Log("******* Replacing UnityWebRequst::DisposeWebRequestCall with wrapper call. *******");
                                            var callWrapperInstruction = iLProcessor.Create(Mono.Cecil.Cil.OpCodes.Call, disposeWebRequestWrapperMethodReference);
                                            iLProcessor.Replace(instructionToReplace, callWrapperInstruction);
                                        }

                                    }
                                }
                            }
                        }

                        // We have not processed this assembly yet
                        if (modifiedAssembly)
                        {
                            Debug.Log("!!!!!! Saving the modified assembly !!!!!!!!");

                            //beginWebRequestMethodDefinition.Body

                            Debug.Log("Injecting empty type definition.");
                            moduleDefinition.Types.Add(new TypeDefinition(string.Empty, "__NRInjected__", Mono.Cecil.TypeAttributes.NotPublic));
                            Debug.Log("Successfully injected empty type definition.");

                            assemblyDefinition.Write();
                        }
                    }
                    else
                    {
                        Debug.Log("Already processed module.");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static IEnumerable<TypeDefinition> GetAllTypesInModule(ModuleDefinition moduleDefinition)
        {
            foreach (var typeDefinition in moduleDefinition.Types)
            {
                yield return typeDefinition;

                if (typeDefinition.HasNestedTypes)
                {
                    foreach (var nestedTypeDefinition in GetAllNestedTypeDefinitions(typeDefinition))
                    {
                        yield return nestedTypeDefinition;
                    }
                }
            }
        }

        private static IEnumerable<TypeDefinition> GetAllNestedTypeDefinitions(TypeDefinition typeDefinition)
        {
            foreach (var nestedTypeDefinition in typeDefinition.NestedTypes)
            {
                yield return nestedTypeDefinition;

                if (nestedTypeDefinition.HasNestedTypes)
                {
                    foreach (var childTypeDefinition in GetAllNestedTypeDefinitions(nestedTypeDefinition))
                    {
                        yield return childTypeDefinition;
                    }
                }
            }
        }
    }

}
#endif
