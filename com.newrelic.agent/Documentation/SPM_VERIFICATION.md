# Verifying Swift Package Manager Resolution

This guide walks through a clean iOS build using Swift Package Manager (SPM)
to confirm the New Relic Unity package's SPM path works end-to-end. It uses
the `Samples → Demo` sample shipped with this package and Unity's standard
iOS build pipeline. No code changes are required.

## Prerequisites

| Tool | Minimum version |
|---|---|
| Unity | 2019.4 LTS or later (SPM verified on 2022 LTS / 6000.0) |
| Xcode | 15+ |
| EDM4U | **1.2.187+** (bundled in this package — do not import an older copy from another plugin) |
| iOS device or simulator | iOS 15+ |

## Steps

1. **Create a fresh Unity project.** Use the 3D template — anything that
   builds for iOS will work.

2. **Install the package.** In *Window → Package Manager*, click the `+` →
   *Add package from git URL* and paste:

   ```
   https://github.com/newrelic/newrelic-unity-agent.git
   ```

3. **Import the demo.** In Package Manager, select `NewRelic SDK` →
   *Samples* tab → *Import* next to `Demo`. The three demo scenes
   (`FirstScene`, `SecondScene`, `ThirdScene`) appear under
   `Assets/Samples/NewRelic SDK/<version>/Demo/`.

4. **Confirm SPM is enabled.** Open *Assets → External Dependency Manager →
   iOS Resolver → Settings*. Verify **Swift Package Manager Enabled** is
   checked (this is the default in 1.2.187). Leave the CocoaPods settings at
   their defaults — they'll be ignored for this package because the SPM
   declaration uses `replacesPod="NewRelicAgent"`.

5. **Set your app token.** Open *Tools → NewRelic → Getting Started*. Switch
   to the *iOS* tab and paste your iOS app token.

6. **Build for iOS.** *File → Build Settings* → switch platform to *iOS* →
   add the demo scene → *Build*. Choose an output folder (e.g. `build/ios`).

7. **Inspect the generated Xcode project.** Open
   `build/ios/Unity-iPhone.xcodeproj`. Confirm:
   - Under *Project navigator → Package Dependencies* you see
     `newrelic-ios-agent-spm` resolved at the version declared in
     `TestUnityDependencies.xml` (currently `7.7.2`).
   - Under *UnityFramework target → General → Frameworks, Libraries, and
     Embedded Content*, the `NewRelic` library is linked.
   - There is **no** `NewRelicAgent` pod in the (optional) Podfile — the
     `replacesPod` attribute suppressed it.

8. **Build & run.** Hit *Run* in Xcode against a real device or the iOS
   simulator. The first build will take longer while Xcode resolves the SPM
   package and downloads the XCFramework.

9. **Verify data.** Launch the app, exercise the demo scene buttons, then
   check New Relic One → *Mobile → \<your iOS app\>* for incoming
   `MobileSession`, `MobileBreadcrumb`, and `Mobile Unity Logs` events.

## Common issues

- **`No such module 'NewRelic'` at compile time** — Xcode hasn't finished
  resolving the SPM package. Wait for the package fetch to complete (status
  bar at top of Xcode) and rebuild.
- **`Multiple commands produce ... NewRelicAgent.framework`** — both
  CocoaPods and SPM are installing the same library. Either uncheck *Swift
  Package Manager Enabled* in EDM4U *or* remove the `Pods/NewRelicAgent`
  folder and let SPM own the dependency. The `replacesPod` attribute should
  prevent this on EDM4U 1.2.187+; if you see it, your EDM4U bundle is older.
- **Older EDM4U imported from another plugin overrides ours** — Unity uses
  the highest-versioned EDM4U it finds across all packages. Check
  *Assets/ExternalDependencyManager* doesn't exist outside this package, or
  upgrade the other plugin's bundled EDM4U.

## Falling back to CocoaPods

If you need to keep using CocoaPods (e.g. to satisfy other plugins),
uncheck *Swift Package Manager Enabled* in iOS Resolver settings. The
existing `<iosPods>` declaration in `TestUnityDependencies.xml` will be
used instead, and the SPM block is ignored.