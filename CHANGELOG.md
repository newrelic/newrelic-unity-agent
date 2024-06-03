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