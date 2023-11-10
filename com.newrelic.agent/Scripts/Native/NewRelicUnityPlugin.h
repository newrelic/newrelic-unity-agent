#import <Foundation/Foundation.h>
#import <NewRelic/NewRelic.h>

#ifdef __cplusplus
extern "C" {
#endif


    extern void NR_logLevel(int logLevel);

    extern void NR_dictionaryDispose(NSMutableDictionary* dictionary);

    extern NSMutableDictionary* NR_dictionaryCreate();

    extern NSMutableArray* NR_ArrayCreate();

    extern void NR_arrayInsertDictionary(NSMutableArray* array, NSMutableDictionary* dic);


    extern void NR_dictionaryInsertString(NSMutableDictionary* dictionary, const char* key, const char* value);

    extern void NR_dictionaryInsertInt64(NSMutableDictionary* dictionary, const char* key, int64_t value);

    extern void NR_dictionaryInsertUInt64(NSMutableDictionary* dictionary, const char* key, uint64_t value);

    extern void NR_dictionaryInsertDouble(NSMutableDictionary* dictionary, const char* key, double value);

    extern void NR_dictionaryInsertFloat(NSMutableDictionary* dictionary, const char* key, float value);

    extern void NR_dictionaryInsertBool(NSMutableDictionary* dictionary, const char* key, bool value);

    extern void NR_dictionaryInsertArray(NSMutableDictionary* dictionary, const char* key, NSMutableArray* value);

    extern char* NR_dictionarygetStringValueByKey(NSMutableDictionary* dictionary, const char* key);


    extern void NewRelic_startWithApplicationToken(const char* appToken);

    extern void NR_crashNow(const char* message);
    extern void NR_enableFeatures(int features);
    extern void NR_disableFeatures(int features);
    extern void NR_enableCrashReporting(bool enabled);
    extern void NR_setApplicationVersion(const char* version);
    extern void NR_setApplicationBuild(const char* buildNumber);
    extern void NR_setPlatform(const char* version);
    extern const char* currentSessionId();


    //Interactions

    extern const char* NR_startInteractionWithName(const char* name);
    extern void NR_stopCurrentInteraction(const char* interactionIdentifier);
    extern void NR_startTracingMethod(const char* methodName, const char* className, NRTimer* tiemr, int category);
    extern void NR_endTracingMethodWithTimer(NRTimer* timer);


    //metrics
    extern void NR_recordMetricsWithName(const char* name, const char* category);
    extern void NR_recordMetricsWithNameValue(const char* name, const char* category, double value);
    extern void NR_recordMetricsWithNameValueUnits(const char* name, const char* category, double value, const char* valueUnits);
    extern void NR_recordMetricsWithNameValueAndCountUnits(const char* name, const char* category, double value, const char* valueUnits, const char* countUnits);


    //Networking

    extern void NR_noticeNetworkRequest(const char* URL,
                                        const char* httpMethod,
                                        int httpStatusCode,
                                        long startTime,
                                        long endTime,
                                        int bytesSent,
                                        int bytesReceived,
                                        const char* responseBody,
                                        NSDictionary* traceAttributes
                                       );


    extern void NR_noticeNetworkFailure(const char* url,
                                        const char* httpMethod,
                                        NRTimer* timer,
                                        int failureCode);

    //Insights Events


    extern void NR_setMaxEventPoolSize(unsigned int size);
    extern void NR_setMaxEventBufferTime(unsigned int seconds);

    extern bool NR_setAttributeStringValue(const char* name, const char* value);
    extern bool NR_setAttributeDoubleValue(const char* name, double value);
    extern bool NR_incrementAttribute(const char* name);
    extern bool NR_incrementAttributeWithValue(const char* name, double amount);

    extern bool NR_removeAttribute(const char* name);
    extern bool NR_removeAllAttributes();
    extern bool NR_recordBreadcrumb(const char* name, NSDictionary* attributes);
    extern bool NR_recordCustomEvent(const char* eventName, NSDictionary* attributes);


    extern void NR_recordHandledExceptionWithStackTrace(NSDictionary* attributes);

    extern void NR_recordHandledExceptionWithStackTrace(NSDictionary* attributes);

    extern NSDictionary* NR_generateDistributedTracingHeaders();

    extern bool NR_setUserId(const char* userId);

    extern void NR_NewRelic_crash();
#ifdef __cplusplus
}
#endif