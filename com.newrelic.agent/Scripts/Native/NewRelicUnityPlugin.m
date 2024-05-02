#import <NewRelic/NewRelic.h>
#import <Foundation/Foundation.h>
#import "NewRelicUnityPlugin.h"
#import <string.h>
#ifdef __cplusplus
extern "C" {
#endif

@interface NewRelic(Development)
+ (void) startTracingMethodNamed:(NSString*)methodName
                     objectNamed:(NSString*)objectName
                           timer:(NRTimer *)timer
                        category:(enum NRTraceType)category;
+ (void) setPlatformVersion:(NSString*)version;
@end

    static bool __useSSL = true;
    extern void NR_useSSL(bool useSSL) {
        __useSSL = useSSL;
    }

    extern bool getUseSSL(){
        return __useSSL;
    }

    extern void NR_dictionaryDispose(NSMutableDictionary* dictionary) {
        if (dictionary) {
            @synchronized(dictionary) {
                //[dictionary release];
            }
        }
    }

    extern NSMutableDictionary* NR_dictionaryCreate(){
        return [NSMutableDictionary new];
    }

    extern NSMutableArray* NR_ArrayCreate(){
      return [NSMutableArray new];
   }

   extern void NR_arrayInsertDictionary(NSMutableArray* array, NSMutableDictionary* dic){
        if (array) {
            @synchronized(array) {
                if (dic == NULL) {
                    return;
                }
                [array addObject:dic];
                
            }
        }
    } 



        extern void NR_dictionaryInsertArray(NSMutableDictionary* dictionary, const char* key, const NSMutableArray* value){
        if (dictionary) {
            @synchronized(dictionary) {
                if (key == NULL) {
                    return;
                }
                NSString* keyString = [NSString stringWithUTF8String:key];
             
                dictionary[keyString] = value;
            }
        }

    }

        extern void NR_dictionaryInsertString(NSMutableDictionary* dictionary, const char* key, const char* value){
        if (dictionary) {
            @synchronized(dictionary) {
                if (key == NULL) {
                    return;
                }
                NSString* keyString = [NSString stringWithUTF8String:key];
                NSString* valueString = value?[NSString stringWithUTF8String:value]:nil;

                dictionary[keyString] = valueString;
            }
        }

    }

      extern char* NR_dictionarygetStringValueByKey(NSMutableDictionary* dict, const char* key){
       if (dict) {
           @synchronized (dict) {
               if (key == NULL) {
                   return "";
               }
                 NSString* keyString = [NSString stringWithUTF8String:key];

                const char* currentSessionId = [dict[keyString] UTF8String];

                char* returnValue = NULL;
                if (currentSessionId != NULL) {
                    returnValue = (char*)malloc(strlen(currentSessionId)+1);
                    strcpy(returnValue, currentSessionId);
                } 

                return returnValue;
           }
       }
       return "";
   }

    extern void NR_crashNow(const char* message) {
        NSString* messageString = message?[NSString stringWithUTF8String:message]:nil;

        if (messageString) {
            [NewRelic crashNow:messageString];
        } else {
            [NewRelic crashNow];
        }
    }
    extern void NR_dictionaryInsertInt64(NSMutableDictionary* dictionary, const char* key, int64_t value){
        if (dictionary) {
            @synchronized(dictionary) {
                if (key == NULL) {
                    return;
                }
                dictionary[[NSString stringWithUTF8String:key]] = @(value);
            }
        }
    }

    extern void NR_logLevel(int logLevel) {
        switch(logLevel) {
            case 0:
                [NRLogger setLogLevels:NRLogLevelNone];
                break;
            case 1:
                [NRLogger setLogLevels:NRLogLevelError];
                break;
            case 2:
                [NRLogger setLogLevels:NRLogLevelWarning];
                break;
            case 3:
                [NRLogger setLogLevels:NRLogLevelInfo];
                break;
            case 4:
                [NRLogger setLogLevels:NRLogLevelVerbose];
                break;
            case 5:
                [NRLogger setLogLevels:NRLogLevelALL];
                break;
            default:
                [NRLogger setLogLevels:NRLogLevelNone];
                break;
        }
    }

    extern void NR_dictionaryInsertUInt64(NSMutableDictionary* dictionary, const char* key, uint64_t value){
        if (dictionary) {
            @synchronized(dictionary) {
                if (key == NULL) {
                    return;
                }
                dictionary[[NSString stringWithUTF8String:key]] = @(value);
            }
        }
    }

    extern void NR_dictionaryInsertDouble(NSMutableDictionary* dictionary, const char* key, double value){
        if (dictionary) {
            @synchronized(dictionary) {
                if (key == NULL) {
                    return;
                }
                dictionary[[NSString stringWithUTF8String:key]] = @(value);
            }
        }
    }

    extern void NR_dictionaryInsertFloat(NSMutableDictionary* dictionary, const char* key, float value){
        if (dictionary) {
            @synchronized(dictionary) {
                if (key == NULL) {
                    return;
                }
                dictionary[[NSString stringWithUTF8String:key]] = @(value);
            }
        }
    }

    extern void NR_dictionaryInsertBool(NSMutableDictionary* dict, const char* key, bool value) {
    if (dict) {
        @synchronized (dict) {
            if (key == NULL) {
                return;
            }
            NSString* keyString = [NSString stringWithUTF8String:key];
            dict[keyString] = @(value);
        }
    }
}

    extern void NR_setPlatform(const char* version){
        [NewRelic setPlatformVersion:[NSString stringWithUTF8String:version]];
        [NewRelic setPlatform:NRMAPlatform_Unity];
    }

    extern void NewRelic_startWithApplicationToken(const char* appToken) {
        NSLog(@"NewRelic_startWithApplicationToken called");
        [NewRelic setPlatform:NRMAPlatform_Unity];
        [NewRelic startWithApplicationToken:[NSString stringWithUTF8String:appToken] withoutSecurity:!getUseSSL()];
    }

    extern NRTimer* NR_createTimer() {
        NRTimer* timer = [NewRelic createAndStartTimer];
       // [timer retain];
        return timer;
    }

    extern void NR_stopTimer(NRTimer* timer){
        if (timer)
        {
            [timer stopTimer];
        }
    }
    extern void NR_disposeTimer(NRTimer* timer){
 /*       if (timer)
        {
           [timer release];
        }
        */
    }
    extern void NR_enableFeatures(int features) {
        [NewRelic enableFeatures:features];
    }

    extern void NR_disableFeatures(int features){
        [NewRelic disableFeatures:features];
    }

    extern void NR_enableCrashReporting(bool enabled){
        [NewRelic enableCrashReporting:(BOOL)enabled];
    }

    extern void NR_setApplicationVersion(const char* version){
        NSString* versionString = version ? [NSString stringWithUTF8String:version]:nil;
        [NewRelic setApplicationVersion:versionString];
    }

    extern void NR_setApplicationBuild(const char* buildNumber){
        [NewRelic setApplicationBuild:[NSString stringWithUTF8String:buildNumber]];
    }

    extern const char* NR_currentSessionId(){
        const char* currentSessionId = [NewRelic currentSessionId].UTF8String;

        char* returnValue = NULL;
        if (currentSessionId != NULL) {
            returnValue = (char*)malloc(strlen(currentSessionId)+1);
            strcpy(returnValue, currentSessionId);
        } 


        return returnValue;
    }



    //Interactions

    extern const char* NR_startInteractionWithName(const char* name){
        const char* interactionStr = [NewRelic startInteractionWithName:[NSString stringWithUTF8String:name]].UTF8String;
        if (interactionStr != NULL) {
            char* returnValue = (char*)malloc(strlen(interactionStr)+1);
            strcpy(returnValue, interactionStr);
            return returnValue;
        }

        return NULL;

    }

    extern void NR_stopCurrentInteraction(const char* interactionIdentifier){
        [NewRelic stopCurrentInteraction:[NSString stringWithUTF8String:interactionIdentifier]];
    }

    extern void NR_startTracingMethod(const char* methodName, const char* className, NRTimer* timer, int category){

        enum NRTraceType traceType = NRTraceTypeNone;
        switch (category) {
            case 0:
                traceType = NRTraceTypeNone;
                break;
            case 1:
                traceType = NRTraceTypeViewLoading;
                break;
            case 2:
                traceType = NRTraceTypeLayout;
                break;
            case 3:
                traceType = NRTraceTypeDatabase;
                break;
            case 4:
                traceType = NRTraceTypeImages;
                break;
            case 5:
                traceType = NRTraceTypeJson;
                break;
            case 6:
                traceType = NRTraceTypeNetwork;
                break;
            default:
                traceType = NRTraceTypeNone;
        }

        NSString* classNameString = className?[NSString stringWithUTF8String:className]:nil;
        NSString* methodNameString = methodName?[NSString stringWithUTF8String:methodName]:nil;


        [NewRelic startTracingMethodNamed:methodNameString
                              objectNamed:classNameString
                                    timer:timer
                                 category:traceType];
    }

    extern void NR_endTracingMethodWithTimer(NRTimer* timer){
        [NewRelic endTracingMethodWithTimer:timer];
    }

    //metrics
    extern void NR_recordMetricsWithName(const char* name, const char* category){
        NSString* nameString = name ? [NSString stringWithUTF8String:name]: nil;
        NSString* categoryString  = category ? [NSString stringWithUTF8String:category]: nil;
        [NewRelic recordMetricWithName:nameString
                              category:categoryString];
    }

    extern void NR_recordMetricsWithNameValue(const char* name, const char* category, double value){

        NSString* nameString = name ? [NSString stringWithUTF8String:name]: nil;
        NSString* categoryString  = category ? [NSString stringWithUTF8String:category]: nil;

        [NewRelic recordMetricWithName:nameString
                              category:categoryString
                                 value:@(value)];
    }

    extern void NR_recordMetricsWithNameValueUnits(const char* name,
                                                   const char* category,
                                                   double value,
                                                   const char* valueUnits){

        NSString* nameString = name ? [NSString stringWithUTF8String:name]: nil;
        NSString* categoryString  = category ? [NSString stringWithUTF8String:category]: nil;
        NSString* valueUnitsString = valueUnits ? [NSString stringWithUTF8String:valueUnits]:nil;

        [NewRelic recordMetricWithName:nameString
                              category:categoryString
                                 value:@(value)
                            valueUnits:valueUnitsString];
    }

    extern void NR_recordMetricsWithNameValueAndCountUnits(const char* name,
                                                           const char* category,
                                                           double value,
                                                           const char* valueUnits,
                                                           const char* countUnits){
        NSString* nameString = name ? [NSString stringWithUTF8String:name]: nil;
        NSString* categoryString  = category ? [NSString stringWithUTF8String:category]: nil;
        NSString* valueUnitsString = valueUnits ? [NSString stringWithUTF8String:valueUnits]:nil;
        NSString* countUnitsString = countUnits ? [NSString stringWithUTF8String:countUnits]:nil;

        [NewRelic recordMetricWithName:nameString
                              category:categoryString
                                 value:@(value)
                            valueUnits:valueUnitsString
                            countUnits:countUnitsString];
    }


    //Networking




    extern void NR_noticeNetworkRequest(const char* URL,
                                        const char* httpMethod,
                                        int httpStatusCode,
                                        long startTime,
                                        long endTime,
                                        int bytesSent,
                                        int bytesReceived,
                                        const char* responseBody,
                                        NSDictionary* traceAttributes) {


      NSString* urlString = URL ? [NSString stringWithUTF8String:URL]:nil;
      NSURL *url = [NSURL URLWithString:urlString];

      NSString* httpMethodString = httpMethod ? [NSString stringWithUTF8String:httpMethod]:nil;
      NSString* responseBodydString = responseBody ? [NSString stringWithUTF8String:responseBody]:nil;
      NSData* data = [responseBodydString dataUsingEncoding:NSUTF8StringEncoding];

        
       [NewRelic noticeNetworkRequestForURL:url httpMethod:httpMethodString startTime:startTime endTime:endTime responseHeaders:nil statusCode:httpStatusCode bytesSent:bytesSent bytesReceived:bytesSent responseData:data traceHeaders:traceAttributes andParams:nil];
    }

    extern void NR_noticeNetworkFailureWithTimer(const char* url,
                                        const char* httpMethod,
                                        NRTimer* timer,
                                        int failureCode) {
        NSString* urlString = url?[NSString stringWithUTF8String:url]:nil;
        NSString* httpMethodString = httpMethod?[NSString stringWithUTF8String:httpMethod]:nil;

        [NewRelic noticeNetworkFailureForURL:urlString?[NSURL URLWithString:urlString]:nil
                                  httpMethod:httpMethodString
                                   withTimer:timer
                              andFailureCode:failureCode];

    }

  extern void NR_noticeNetworkFailure(const char* url,
                                        const char* httpMethod,
                                        long startTime,
                                        long endTime,
                                        int failureCode) {
        NSString* urlString = url?[NSString stringWithUTF8String:url]:nil;
        NSString* httpMethodString = httpMethod?[NSString stringWithUTF8String:httpMethod]:nil;

        [NewRelic noticeNetworkFailureForURL:urlString?[NSURL URLWithString:urlString]:nil
                                  httpMethod:httpMethodString
                                  startTime:startTime
                                  endTime:endTime
                              andFailureCode:failureCode];

    }

    //Insights Events


    extern void NR_setMaxEventPoolSize(unsigned int size){
        return [NewRelic setMaxEventPoolSize:size];
    }

    extern void NR_setMaxEventBufferTime(unsigned int seconds){
        [NewRelic setMaxEventBufferTime:seconds];
    }

     extern void NR_setMaxOfflineStorageSize(unsigned int megabytes){
        [NewRelic setMaxOfflineStorageSize:megabytes];
    }

    extern bool NR_setAttributeStringValue(const char* named, const char* value){
        NSString* nameString = named ? [NSString stringWithUTF8String:named]:nil;
        NSString* valueString = value ? [NSString stringWithUTF8String:value]:nil;

        return [NewRelic setAttribute:nameString
                                value:valueString];
    }

    extern bool NR_setAttributeDoubleValue(const char* name, double value){
        NSString* nameString = name ? [NSString stringWithUTF8String:name]:nil;
        return [NewRelic setAttribute:nameString
                                value:@(value)];
    }


    extern bool NR_incrementAttribute(const char* name){
        NSString* nameString = name ? [NSString stringWithUTF8String:name]:nil;
        return [NewRelic incrementAttribute:nameString];
    }

    extern bool NR_incrementAttributeWithValue(const char* name, double amount){
        NSString* nameString = name ? [NSString stringWithUTF8String:name]:nil;
        return [NewRelic incrementAttribute:nameString
                                      value:@(amount)];
    }
    
    extern bool NR_removeAttribute(const char* name){
        NSString* nameString = name ? [NSString stringWithUTF8String:name]:nil;
        return [NewRelic removeAttribute:nameString];
    }
    
    extern bool NR_removeAllAttributes(){
        return [NewRelic removeAllAttributes];
    }

    extern bool NR_recordBreadcrumb(const char* name, NSDictionary* attributes) {
    NSString* nameString = name ? [NSString stringWithUTF8String:name] : nil;
    return [NewRelic recordBreadcrumb:nameString attributes:attributes];
    }
        extern bool NR_recordCustomEvent(const char* eventName, NSDictionary* attributes) {
        NSString* nameString = eventName ? [NSString stringWithUTF8String:eventName] : nil;
        return [NewRelic recordCustomEvent:nameString attributes:attributes];
    }


    extern void NR_recordHandledExceptionWithStackTrace(NSDictionary* attributes){
       [NewRelic recordHandledExceptionWithStackTrace:attributes];
    }

    extern NSDictionary* NR_generateDistributedTracingHeaders(){
        
        return [NewRelic generateDistributedTracingHeaders];
    }


     extern bool NR_setUserId(const char* userId){
        return [NewRelic setUserId:[NSString stringWithUTF8String:userId]];
    }

#ifdef __cplusplus
}
#endif