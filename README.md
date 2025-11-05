<a href="https://opensource.newrelic.com/oss-category/#community-project"><picture><source media="(prefers-color-scheme: dark)" srcset="https://github.com/newrelic/opensource-website/raw/main/src/images/categories/dark/Community_Project.png"><source media="(prefers-color-scheme: light)" srcset="https://github.com/newrelic/opensource-website/raw/main/src/images/categories/Community_Project.png"><img alt="New Relic Open Source community project banner." src="https://github.com/newrelic/opensource-website/raw/main/src/images/categories/Community_Project.png"></picture></a>


# New Relic Unity Agent

This agent utilizes the native New Relic Android and iOS agents to instrument Unity apps. The New Relic SDKs gather data such as crashes, network traffic, and other relevant information to help monitor and assess the performance of Unity apps.

## Features
* Record and Capture C# errors
* Network Instrumentation
* Distributed Tracing 
* Tracking UnityEngine Debug log, assert and error
* Handled Exception
* Capture interactions and the sequence in which they were created
* Pass user information to New Relic to track user sessions
* Scene Navigation as Interactions
* Capture Native C++ Errors
* offline monitoring of events and exceptions
* Capture Background Events when app is in background

## Current Support:
- Android API 24+ (AGP 7 and Higher)
- iOS 10
- Depends on New Relic iOS/XCFramework and Android agents

## Installation

1. Scoped Registries allow Unity to communicate the location of any custom package registry server to the Package Manager so that the user has access to several collections of packages at the same time. NewRelic uses Scoped 
   Registries to allow our users to manage, download and install our SDK using the built-in Unity Package Manager.

   In the Package Manager in the Unity IDE, download the NewRelic SDK using add package from git url

   ```
   https://github.com/newrelic/newrelic-unity-agent.git
   
   ```
   
<img width="801" alt="Screenshot 2023-11-27 at 2 03 05 PM" src="https://github.com/newrelic/newrelic-unity-agent/assets/89222514/480fdc95-d7c5-4693-9aca-09998a211609">

2. Open the NewRelic editor

In your Unity IDE, click Tools → NewRelic → Getting Started to open the NewRelic editor window.
<img width="622" alt="Screenshot 2023-07-13 at 1 07 46 PM" src="https://github.com/ndesai-newrelic/newrelic-unity-agent/assets/89222514/2691a4a0-b0a0-4f4a-8532-f8f1e7975a6e">

3. Update your app information on the editor
   Select Android and enter the App token:

   AppToken is platform-specific. You need to generate the seprate token for Android and iOS apps to get better Visibility at app level.


4. External Dependency Manager support (do not skip this step!)
   If using the Unity External Dependency Manager plug-in, disable the your dependency resolver at the root level in **launcherTemplate.gradle**:

```groovy
 apply plugin: 'com.android.application'
 apply plugin: 'newrelic' // <-- add this
dependencies {
    implementation project(':unityLibrary')
    implementation 'com.newrelic.agent.android:agent-ndk:1.1.1' 
    implementation 'com.newrelic.agent.android:android-agent:7.6.12' 
    }

android {
    compileSdkVersion **APIVERSION**
    buildToolsVersion '**BUILDTOOLS**'

```

5. Customize Gradle Templates
   If using Unity 2019 or later, add the following to your Gradle files:

   1.Include the New Relic Maven repository URL in the Gradle build settings. To do this, open your **mainTemplate.gradle** file (usually located in Assets/Plugins/Android folder) and add the New Relic Maven URL like this:

```groovy
   allprojects {
    buildscript {
        repositories {**ARTIFACTORYREPOSITORY**
            google()
            jcenter()
            mavenCentral()
        }
```
  2. Add the New Relic classpath to your project-level **baseProjectTemplate.gradle** file (typically located in the android folder in your Unity project):


  ```groovy
        dependencies {
            // If you are changing the Android Gradle Plugin version, make sure it is compatible with the Gradle version preinstalled with Unity
            // See which Gradle version is preinstalled with Unity here https://docs.unity3d.com/Manual/android-gradle-overview.html
            // See official Gradle and Android Gradle Plugin compatibility table here https://developer.android.com/studio/releases/gradle-plugin#updating-gradle
            // To specify a custom Gradle version in Unity, go to "Preferences > External Tools", uncheck "Gradle Installed with Unity (recommended)" and specify a path to a custom Gradle version
            classpath 'com.newrelic.agent.android:agent-gradle-plugin:7.6.12'
            **BUILD_SCRIPT_DEPS**
        }
    }


If you are utilizing an older version of Unity Studio, you can incorporate a lower version of the classpath as a dependency which supports AGP7 and lower version of Gradle.

```groovy
        dependencies {
            // If you are changing the Android Gradle Plugin version, make sure it is compatible with the Gradle version preinstalled with Unity
            // See which Gradle version is preinstalled with Unity here https://docs.unity3d.com/Manual/android-gradle-overview.html
            // See official Gradle and Android Gradle Plugin compatibility table here https://developer.android.com/studio/releases/gradle-plugin#updating-gradle
            // To specify a custom Gradle version in Unity, go to "Preferences > External Tools", uncheck "Gradle Installed with Unity (recommended)" and specify a path to a custom Gradle version
            classpath 'com.newrelic.agent.android:agent-gradle-plugin:7.6.12'
            **BUILD_SCRIPT_DEPS**
        }
    }

 ```
  By making these changes in your Gradle files, you will ensure that the New Relic artifacts are properly downloaded and included in your Unity project.


6. Please ensure that your External Dependency Manager settings match the following configuration. In your Unity IDE, navigate to Assets → External Dependency Manager → iOS Resolver → Settings:

   1. Add use_frameworks! to Podfile is unchecked.
   2. Always add the main target to Podfile box is checked.

  <img width="407" alt="Screenshot 2023-07-13 at 1 22 21 PM" src="https://github.com/ndesai-newrelic/newrelic-unity-agent/assets/89222514/5de6fb36-f60d-4470-a1c6-78975d4c4a10">

7. If the Podfile is not being used for iOS dependency management, you can proceed with the following steps.

   1. Download and unzip the New Relic XCFramework SDK
    Download the latest iOS agent from our [iOS agent release notes](https://docs.newrelic.com/docs/release-notes/mobile-release-notes/ios-release-notes)
   2. Add the New Relic XCFramework to your Xcode project
     Unzip the SDK download, drag the “NewRelicAgent.xcframework” folder from the Finder into your Xcode project (dropping it onto your Targets Frameworks pane). Select “Embed & Sign” under the Embed column.



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

### [noticeNetworkFailure](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile-android/android-sdk-api/notice-network-failure)(string url,string httpMethod,long startTime,long endTime,NewRelicAgent.NetworkFailureCode failureCode,string message): void; or (string url,string httpMethod,Timer timer,NewRelicAgent.NetworkFailureCode failureCode,string message): void; 
> Records network failures. If a network request fails, use this method to record details about the failures. In most cases, place this call inside exception handlers, such as catch blocks.
```C#
    long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    NewRelic.noticeNetworkFailure('https://github.com', 'GET', startTime, endTime, NewRelic.NetworkFailure.BadURL);

    Timer timer = new();
    NewRelic.noticeNetworkFailure('https://github.com', 'GET',timer , NewRelic.NetworkFailure.BadURL);
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
> Records C# errors for Unity.
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

### [setMaxOfflineStorageSize](https://docs.newrelic.com/docs/mobile-monitoring/new-relic-mobile/mobile-sdk/set-max-offline-storage/)(uint megabytes): void;
> Sets the maximum size of total data that can be stored for offline storage.By default, mobile monitoring can collect a maximum of 100 megaBytes of offline storage. When a data payload fails to send because the device doesn't have an internet connection, it can be stored in the file system until an internet connection has been made. After a typical harvest payload has been successfully sent, all offline data is sent to New Relic and cleared from storage.
```C#
    NewRelicAgent.setMaxOfflineStorageSize(200);
```

### LogInfo(String message) : void

> Logs an informational message to the New Relic log.
``` C#
    NewRelicAgent.LogInfo("This is an informational message");
```

### LogError(String message) : void
> Logs an error message to the New Relic log.
``` C#
    NewRelicAgent.LogError("This is an error message");
```
### LogVerbose(String message) : void
> Logs a verbose message to the New Relic log.
``` C#
    NewRelicAgent.LogVerbose("This is a verbose message");
```

### LogWarning(String message) : void
> Logs a warning message to the New Relic log.
``` C#
    NewRelicAgent.LogWarning("This is a warning message");
```

### LogDebug(String message) : void
> Logs a debug message to the New Relic log.
``` C#
    NewRelicAgent.LogDebug("This is a debug message");
```

### Log(NewRelicAgent.AgentLogLevel level, String message) : void
> Logs a message to the New Relic log with a specified log level.
``` C#
    NewRelicAgent.LogNewRelic.NewRelicAgent.AgentLogLevel.INFO, "This is an informational message");
``` 

### LogAttributes(Dictionary<string, object> attributes) : void
> Logs a message with attributes to the New Relic log.
``` C#
    NewRelicAgent.LogAttributes(new Dictionary<string, object>()
        {
            {"BreadNumValue", 12.3 },
            {"BreadStrValue", "UnityBread" },
            {"BreadBoolValue", true },
            {"message", "This is a message with attributes" }
        }
    );
```

## How to see C# Errors(Fatal/Non Fatal) in NewRelic One?

C# errors and handled exceptions can be seen in the `Handled Exceptions` tab in New Relic One. You will be able to see the event trail, attributes, and stack trace for each C# error recorded.

You can also build a dashboard for these errors using this query:

```sql
SELECT * FROM MobileHandledException SINCE 24 hours ago
```
## Query Unity log data [#logs]

New Relic stores your Unity logs as custom events. You can query these logs and build dashboards for them using this NRQL query:

```nrql
 SELECT * FROM `Mobile Unity Logs` SINCE 30 MINUTES AGO
```

For more information on NRQL queries, see [Introduction to NRQL](/docs/query-your-data/nrql-new-relic-query-language/get-started/introduction-nrql-new-relics-query-language/#where).

## Contribute

We encourage your contributions to improve newrelic-unity-agent! Keep in mind that when you submit your pull request, you'll need to sign the CLA via the click-through using CLA-Assistant. You only have to sign the CLA one time per project.

If you have any questions, or to execute our corporate CLA (which is required if your contribution is on behalf of a company), drop us an email at opensource@newrelic.com.

**A note about vulnerabilities**

As noted in our [security policy](../../security/policy), New Relic is committed to the privacy and security of our customers and their data. We believe that providing coordinated disclosure by security researchers and engaging with the security community are important means to achieve our security goals.

If you believe you have found a security vulnerability in this project or any of New Relic's products or websites, we welcome and greatly appreciate you reporting it to New Relic through [our bug bounty program](https://docs.newrelic.com/docs/security/security-privacy/information-security/report-security-vulnerabilities/).

If you would like to contribute to this project, review [these guidelines](./CONTRIBUTING.md).

To all contributors, we thank you!  Without your contribution, this project would not be what it is today.  We also host a community project page dedicated to [Project Name](<LINK TO https://opensource.newrelic.com/projects/... PAGE>).

## Support

New Relic hosts and moderates an online forum where customers, users, maintainers, contributors, and New Relic employees can discuss and collaborate:

[forum.newrelic.com](https://forum.newrelic.com/).

## License
newrelic-unity-agent is licensed under the [Apache 2.0](http://apache.org/licenses/LICENSE-2.0.txt) License.
>[If applicable: newrelic-unity-agent also uses source code from third-party libraries. Full details on which libraries are used and the terms under which they are licensed can be found in the third-party notices document.]
