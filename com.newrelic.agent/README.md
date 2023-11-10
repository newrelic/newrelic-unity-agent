[![Community Plus header](https://github.com/newrelic/opensource-website/raw/main/src/images/categories/Community_Plus.png)](https://opensource.newrelic.com/oss-category/#community-plus)


# New Relic Unity Agent

This agent utilizes the native New Relic Android and iOS agents to instrument Unity apps. The New Relic SDKs gather data such as crashes, network traffic, and other relevant information to help monitor and assess the performance of Unity apps.

## Features
* Record and Capture C# errors
* Network Instrumentation (UnityWebrequest Android Support in Future) 
* Distributed Tracing (UnityWebrequest Android Support in Future) 
* Tracking UnityEngine Debug log, assert and error
* Handled Exception
* Capture interactions and the sequence in which they were created
* Pass user information to New Relic to track user sessions
* Scene Navigation as Interactions
* Capture Native C++ Errors

## Current Support:
- Android API 24+
- iOS 10
- Depends on New Relic iOS/XCFramework and Android agents

## Android Installation

1. Downloading the package
   If you haven’t already done so, [download the NewRelic SDK for Unity](https://github.com/ndesai-newrelic/newrelic-unity-agent/blob/unity_without_framework/NewRelic.unitypackage).

2. Importing the package
   In your Unity IDE, click Assets → Import Package → Custom Package and Import the NewRelic package.
  <img width="624" alt="Screenshot 2023-07-13 at 1 07 02 PM" src="https://github.com/ndesai-newrelic/newrelic-unity-agent/assets/89222514/22fb6d19-bf90-446e-a560-6a95815a094e">

3. Open the NewRelic editor

In your Unity IDE, click Tools → NewRelic → Getting Started to open the NewRelic editor window.
<img width="622" alt="Screenshot 2023-07-13 at 1 07 46 PM" src="https://github.com/ndesai-newrelic/newrelic-unity-agent/assets/89222514/2691a4a0-b0a0-4f4a-8532-f8f1e7975a6e">

4. Update your app information on the editor
   Select Android and enter the App token:

5. External Dependency Manager support (do not skip this step!)
   If using the Unity External Dependency Manager plug-in, disable the NewRelic dependency resolver at the root level in **launcherTemplate.gradle**:

```groovy
 apply plugin: 'com.android.application'
 **apply plugin: 'newrelic'**
dependencies {
    implementation project(':unityLibrary')
    implementation 'com.google.android.gms:play-services-games:9.8.0' // Assets/Plugins/Editor/TestUnityDependencies.xml:9
    implementation 'com.newrelic.agent.android:agent-ndk:1.0.3' // Assets/Plugins/Editor/TestUnityDependencies.xml:11
    implementation 'com.newrelic.agent.android:android-agent:7.0.1' // Assets/Plugins/Editor/TestUnityDependencies.xml:13
    }

android {
    compileSdkVersion **APIVERSION**
    buildToolsVersion '**BUILDTOOLS**'

```

6. Customize Gradle Templates
   If using Unity 2019 or later, add the following to your Gradle files:

   1.Include the New Relic Maven repository URL in the Gradle build settings. To do this, open your **mainTemplate.gradle** file (usually located in Assets/Plugins/Android folder) and add the New Relic Maven URL like this:

```groovy
   allprojects {
    buildscript {
        repositories {**ARTIFACTORYREPOSITORY**
            google()
            jcenter()
            maven {
               url "https://oss.sonatype.org/content/repositories/comnewrelic-2758"
            }
        }
```
  2. Add the New Relic classpath to your project-level **baseProjectTemplate.gradle** file (typically located in the android folder in your Unity project):
```groovy
        dependencies {
            // If you are changing the Android Gradle Plugin version, make sure it is compatible with the Gradle version preinstalled with Unity
            // See which Gradle version is preinstalled with Unity here https://docs.unity3d.com/Manual/android-gradle-overview.html
            // See official Gradle and Android Gradle Plugin compatibility table here https://developer.android.com/studio/releases/gradle-plugin#updating-gradle
            // To specify a custom Gradle version in Unity, go do "Preferences > External Tools", uncheck "Gradle Installed with Unity (recommended)" and specify a path to a custom Gradle version
            classpath 'com.android.tools.build:gradle:4.0.1'
            classpath 'com.newrelic.agent.android:agent-gradle-plugin:7.0.1'
            **BUILD_SCRIPT_DEPS**
        }
    }

 ```
  By making these changes in your Gradle files, you will ensure that the New Relic artifacts are properly downloaded and included in your Unity project.

7.Make sure your app requests INTERNET and ACCESS_NETWORK_STATE permissions by adding these lines to your AndroidManifest.xml

 ``` xml
    <uses-permission android:name="android.permission.INTERNET" />
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
  ```

8. Please ensure that your External Dependency Manager settings match the following configuration. In your Unity IDE, navigate to Assets → External Dependency Manager → iOS Resolver → Settings:

<img width="407" alt="Screenshot 2023-07-13 at 1 22 21 PM" src="https://github.com/ndesai-newrelic/newrelic-unity-agent/assets/89222514/5de6fb36-f60d-4470-a1c6-78975d4c4a10">

## Usage
See the examples below, and for more detail, see [New Relic IOS SDK doc](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-ios/ios-sdk-api) or [Android SDK](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api).

### [startInteractionWithName](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/start-interaction)(string name): &lt;InteractionId&gt;;
> Track a method as an interaction.

`InteractionId` is string.


### [stopCurrentInteraction](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/end-interaction)(string interactionIdentifier): void;
> End an interaction
> (Required). This uses the string ID for the interaction you want to end.
> This string is returned when you use startInteraction().

  ```C#

        string interActionId = NewRelicAgent.StartInteractionWithName("Unity InterAction Example");

        for(int i =0; i < 4;i++)
        {
            Thread.Sleep(1000);
        }

        NewRelicAgent.StopCurrentInteraction(interActionId);
  
  ```

### [setAttribute](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/set-attribute)(string name, string|double value): void;
> Creates a session-level attribute shared by multiple mobile event types. Overwrites its previous value and type each time it is called.
  ```C#
     NewRelicAgent.setAttribute('UnityCustomAttrNumber', 37);
  ```
### [removeAttribute](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/remove-attribute)(string name): void;
> This method removes the attribute specified by the name string..
  ```C#
     NewRelicAgent.removeAttribute('UnityCustomAttrNumber');
  ```

### [incrementAttribute](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/increment-attribute)(string name, double amount): void;
> Increments the count of an attribute with a specified name. Overwrites its previous value and type each time it is called. If the attribute does not exists, it creates a new attribute. If no value is given, it increments the value by 1.
```C#
    NewRelicAgent.incrementAttribute('UnityCustomAttrNumber');
    NewRelicAgent.incrementAttribute('UnityCustomAttrNumber', 5);
```

### [setUserId](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/set-user-id)(string userId): void;
> Set a custom user identifier value to associate user sessions with analytics events and attributes.
  ```C#
     NewRelicAgent.setUserId("Unity12934");
  ```

### [recordBreadcrumb](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/recordbreadcrumb)(string name, Dictionary<string, object> attributes): bool;
> Track app activity/screen that may be helpful for troubleshooting crashes.

  ```C#
        Dictionary<string, object> dic = new Dictionary<string, object>();
        dic.Add("Unity Attribute", "Data1");

        NewRelicAgent.RecordBreadCrumb("Unity BreadCrumb Example", dic);
  ```

### [recordCustomEvent](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/recordcustomevent-android-sdk-api)(string name, Dictionary<string, object> attributes): bool;
> Creates and records a custom event for use in New Relic Insights.

  ```C#
        Dictionary<string, object> dic = new Dictionary<string, object>();
        dic.Add("Unity Custom Attribute", "Data2");

        NewRelicAgent.RecordCustomEvent("Unity Custom Event Example", dic);
  ```


### [currentSessionId](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/currentsessionid-android-sdk-api)(): string;
> Returns the current session ID. This method is useful for consolidating monitoring of app data (not just New Relic data) based on a single session definition and identifier.
```C#
    string sessionId =  NewRelicAgent.currentSessionId();
```

### [noticeHttpTransaction](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/notice-http-transaction/)(string httpMethod, int statusCode,long startTime,long endTime,long bytesSent,long bytesReceived,string responseBody, Dictionary<string,object> dtHeaders): void;
> Tracks network requests manually. You can use this method to record HTTP transactions, with an option to also send a response body.
```C#
    NewRelicAgent.noticeHttpTransaction('https://github.com', 'GET', 200, DateTimeOffset.Now.ToUnixTimeMilliseconds(), DateTimeOffset.Now.ToUnixTimeMilliseconds()+1000, 100, 101, "response body",null);
```


### [recordMetric](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/recordmetric-android-sdk-api)(string name, string category,double value, NewRelicAgent.MetricUnit valueUnits,NewRelicAgent.MetricUnit countUnits): void;
> Records custom metrics (arbitrary numerical data), where countUnit is the measurement unit of the metric count and valueUnit is the measurement unit for the metric value. If using countUnit or valueUnit, then all of value, countUnit, and valueUnit must all be set.
```C#
    NewRelicAgent.recordMetricWithName('UnityCustomMetricName', 'UnityCustomMetricCategory');
    NewRelicAgent.recordMetricWithName('UnityCustomMetricName', 'UnityCustomMetricCategory', 12);
    NewRelicAgent.recordMetricWithName('UnityCustomMetricName', 'UnityCustomMetricCategory', 13, NewRelicAgent.MetricUnit.PERCENT, NewRelicAgent.MetricUnit.SECONDS);
```

### [removeAllAttributes](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/remove-all-attributes)(): void;
> Removes all attributes from the session
```C#
    NewRelicAgent.RemoveAllAttributes();
```

### recordError(e: string|error): void;
> Records javascript errors for react-native.
```C#
    try {
      string foo;
      foo.Length;
    } catch (Exception e)
    {
        NewRelicAgent.RecordException(e);
    }
```

### [setMaxEventBufferTime](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/set-max-event-buffer-time)(uint seconds): void;
> Sets the event harvest cycle length. Default is 600 seconds (10 minutes). Minimum value can not be less than 60 seconds. Maximum value should not be greater than 600 seconds.
```C#
    NewRelicAgent.setMaxEventBufferTime(60);
```

### [setMaxEventPoolSize](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/set-max-event-pool-size)(uint size): void;
> Sets the maximum size of the event pool stored in memory until the next harvest cycle. Default is a maximum of 1000 events per event harvest cycle. When the pool size limit is reached, the agent will start sampling events, discarding some new and old, until the pool of events is sent in the next harvest cycle.
```C#
    NewRelicAgent.setMaxEventPoolSize(2000);
```



## How to see C# Errors(Fatal/Non Fatal) in NewRelic One?

C# errors and handled exceptions can be seen in the `Handled Exceptions` tab in New Relic One. You will be able to see the event trail, attributes, and stack trace for each C# error recorded.

You can also build a dashboard for these errors using this query:

```sql
SELECT * FROM MobileHandledException SINCE 24 hours ago
```

## How can UnityWebRequest be instrumented for Android apps?

We currently do not offer automatic instrumentation for UnityWebRequest in Android apps. However, you can perform manual instrumentation by following these instructions.

```C#


        Dictionary<string, object> dtHeaders = NewRelicAgent.NoticeDistributedTrace();

        Dictionary<string, object> traceAttributes = new Dictionary<string, object>();



        long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        using (UnityWebRequest request = UnityWebRequest.Get("http://unity3d.com/"))
        {

            foreach (var header in dtHeaders)
            {
                if (header.Key.Equals(NRConstants.TRACE_PARENT) || header.Key.Equals(NRConstants.TRACE_STATE) || header.Key.Equals(NRConstants.NEWRELIC))
                {
                    request.SetRequestHeader(header.Key, header.Value.ToString());
                    traceAttributes.Add(header.Key, header.Value.ToString());
                }
#if UNITY_ANDROID

                if (header.Key.Equals("trace.id") || header.Key.Equals("guid") || header.Key.Equals("id"))
                {
                    traceAttributes.Add(header.Key, header.Value.ToString());
                }
#endif

            }

            yield return request.SendWebRequest();

            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (request.isNetworkError) // Error
            {
               UnityEngine.Debug.Log(request.error);
            }
            else // Success
            {
                UnityEngine.Debug.Log(request.downloadHandler.text);
            }

           #if UNITY_ANDROID

            NewRelicAgent.NoticeHttpTransaction("http://unity3d.com", "GET", Convert.ToInt32(request.responseCode), startTime, endTime, 0, request.result.ToString().Length,request.result.ToString(), traceAttributes);
           #endif


        }

  ```

