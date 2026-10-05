## 1.7.2

## Improvements
- Native Android agent updated to version 7.8.3
- Native iOS agent updated to version 7.7.7


## 1.7.1

## Improvements
- Native iOS agent updated to version 7.7.7


## 1.7.0

## Bug fixes
- Fixed iOS apps crashing on launch with `Library not loaded: @rpath/NewRelic.framework/NewRelic`. The Swift Package Manager declaration added in 1.6.0 has been removed and iOS dependencies now resolve via CocoaPods only. Builds on 1.6.0 through 1.6.4 were affected unless Swift Package Manager was manually disabled in the EDM4U iOS Resolver settings.
  EDM4U's SPM resolver links package products against the `UnityFramework` target without embedding
  them into the built `.app`, so the dynamic `NewRelic.xcframework` was never present at runtime.
  Because EDM4U enables SPM by default, this was the default iOS path on those versions. See
  [#82](https://github.com/newrelic/newrelic-unity-agent/issues/82) and the upstream report
  [googlesamples/unity-jar-resolver#779](https://github.com/googlesamples/unity-jar-resolver/issues/779).
- Fixed the New Relic iOS agent being missing entirely from builds on Unity 2019.1 through 2021.2, where EDM4U suppressed the `NewRelicAgent` pod but skipped Swift Package Manager injection because it requires Unity 2021.3 or newer.

## Improvements
- Native Android agent updated to version 7.8.2
- Native Android NDK agent updated to version 1.1.5
- Native iOS agent updated to version 7.7.6

## Upgrade notes
- iOS dependencies now resolve through CocoaPods only. No action is needed if you previously used Swift Package Manager, as EDM4U falls back to the `NewRelicAgent` pod automatically and the **Swift Package Manager Enabled** setting can be left checked.
- If a generated Xcode project still carries a `newrelic-ios-agent-spm` package reference from an earlier build, delete the generated project and rebuild from Unity to clear it.


## 1.6.4

## Improvements
- Native Android agent updated to version 7.8.1
- Native iOS agent updated to version 7.7.6


## 1.6.3

## Improvements
- Native Android agent updated to version 7.8.0
- Native iOS agent updated to version 7.7.5


## 1.6.2

## Improvements
- Native Android agent updated to version 7.7.7
- Native iOS agent updated to version 7.7.3


## 1.6.1

## Improvements
- Native Android agent updated to version 7.7.6
- Native iOS agent updated to version 7.7.2


## 1.6.0

## New Features
- iOS dependencies can now be resolved via Swift Package Manager. The package's
  `TestUnityDependencies.xml` declares both a `<remoteSwiftPackage>` block
  pointing at [`newrelic/newrelic-ios-agent-spm`](https://github.com/newrelic/newrelic-ios-agent-spm)
  and the existing `<iosPod>` block. EDM4U automatically suppresses the pod
  via `replacesPod` when SPM is enabled. See `Documentation/SPM_VERIFICATION.md`.

  > **Retracted in 1.7.0.** This path crashes iOS apps on launch and was removed.
  > Do not use 1.6.0–1.6.4 with Swift Package Manager enabled. See the 1.7.0 notes.

## Improvements
- Bundled External Dependency Manager for Unity (EDM4U) upgraded from 1.2.175
  to 1.2.187 (required for Swift Package Manager support).

## 1.5.3

## Improvements
- Native Android agent updated to version 7.7.5
- Native iOS agent updated to version 7.7.1


## 1.5.2

## Improvements
- Native Android agent updated to version 7.7.4
- Native iOS agent updated to version 7.7.1


## 1.5.1

## Improvements
- Native Android agent updated to version 7.7.2
- Native iOS agent updated to version 7.7.1


## 1.5.0

## Improvements
- Native iOS agent updated to version 7.7.0


## 1.4.15

## Improvements
- Native Android agent updated to version 7.7.1

## 1.4.14

## Improvements
- Native Android agent updated to version 7.7.0
- Native iOS agent updated to version 7.6.3


## 1.4.13

## Improvements
- Native Android agent updated to version 7.6.15
- Native iOS agent updated to version 7.6.1


## 1.4.12

## Improvements
- Native Android agent updated to version 7.6.13
- Native iOS agent updated to version 7.6.0


## 1.4.11

## Improvements
- Native Android agent updated to version 7.6.12
- Native iOS agent updated to version 7.6.0


## 1.4.10

## Improvements
- Native Android agent updated to version 7.6.10
- Native iOS agent updated to version 7.5.11

## 1.4.9

## Improvements
- Native Android agent updated to version 7.6.8
- Native iOS agent updated to version 7.5.8
- fixed Unity Exception stack traces issue when namespaces are used.

## 1.4.8

## Improvements
- Native Android agent updated to version 7.6.7
- Native iOS agent updated to version 7.5.6

## 1.4.7

## Improvements
- Native Android agent updated to version 7.6.6
- Native iOS agent updated to version 7.5.5

## 1.4.6

## Improvements
- Native Android agent updated to version 7.6.5

## 1.4.5

## Improvements
- Native iOS agent updated to version 7.5.4

## 1.4.4

## Improvements
- Native Android agent updated to version 7.6.4

## 1.4.3


## Improvements

- Native Android agent updated to version 7.6.2
- Native iOS agent updated to version 7.5.3

## 1.4.2


## Improvements

- Native Android agent updated to version 7.6.1

## 1.4.1


## Improvements

- Native Android agent updated to version 7.6.0
- Native iOS agent updated to version 7.5.2
- Bug fixes for Log Attributes method

## 1.4.0

## New Features

1. Application Exit Information
  - Added ApplicationExitInfo to data reporting
  - Enabled by default

2. Log Forwarding to New Relic
  - Implement static API for sending logs to New Relic
  - Can be enabled/disabled in your mobile application's entity settings page

## Improvements

- Native Android agent updated to version 7.5.0
- Native iOS agent updated to version 7.5.0

## 1.3.6

* Improvements

The native iOS Agent has been updated to version 7.4.11, bringing performance enhancements and bug fixes.

* New Features

A new backgroundReportingEnabled feature flag has been introduced to enable background reporting functionality.
A new newEventSystemEnabled feature flag has been added to enable the new event system.

* Bug Fixes
Resolved a problem where customers encountered a mono linker build failure when using the New Relic agent.

## 1.3.5

- To address the issue of crashes occurring when using the NoticeFailure method on background threads, we have added StartTime and EndTime parameters to the method. This enhancement should prevent such crashes from happening.
- Upgraded native iOS Agent to 7.4.11
- Upgraded native Android Agent to 7.3.0

## 1.3.4

- Upgraded native iOS Agent to 7.4.10

## 1.3.3

- Offline Harvesting Feature: Preserves harvest data during internet downtime, sending stored data once online.

- setMaxOfflineStorageSize API: Allows setting a maximum limit for local data storage.

- Upgraded native iOS Agent to 7.4.9: Offers performance upgrades and bug fixes.

- Upgraded native Android Agent to 7.3.0: Improves stability and adds enhanced features.

- UnityWebRequest Instrumentation Update: Fixes issue with replacement of constrained dispose calls, streamlining app building.

## 1.3.2

- Resolved an issue in the Unity editor where an "assembly not found" error occurred for the New Relic native integration on Windows, Mac, and web platforms. 

## 1.3.1

- Resolved an issue in UnityWebRequest instrumentation where "callvirt" instructions were erroneously replaced with "call" instructions.

## 1.3.0

- Resolved an issue with the application framework that was causing unexpected behavior.
- Fixed a bug in the application log received handler, ensuring accurate and reliable logging.
- Addressed a build issue that was causing problems with the iOS app.

## 1.0.0

🎉🎊 Presenting the new NewRelic SDK for Unity:

Allows instrumenting Unity apps and getting valuable insights in the NewRelic UI. Features:
request tracking, error/crash reporting,distributed tracing, info points, and many more. Thoroughly
maintained and ready for production.
