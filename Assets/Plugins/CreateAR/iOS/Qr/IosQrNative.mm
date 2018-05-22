//
//  IosQrNativeInterface.m
//  Unity-iPhone
//
//  Created by Benjamin Jordan on 5/15/18.
//

#include "stdlib.h"
#include <AVFoundation/AVFoundation.h>
#include <Foundation/Foundation.h>

extern "C"
{
    char* cStringCopy(const char* string)
    {
        if (string == NULL)
            return NULL;
        
        char* res = (char*)malloc(strlen(string) + 1);
        strcpy(res, string);
        
        return res;
    }
    
    char* unity_startDecoding(const char* stringValue)
    {
        NSString* path = [NSString stringWithUTF8String: stringValue];
        
        CIDetector* detector = [CIDetector detectorOfType:CIDetectorTypeQRCode context:nil options:@{CIDetectorTracking: @YES, CIDetectorAccuracy: CIDetectorAccuracyHigh}];
        NSArray* features = [detector featuresInImage:[CIImage imageWithCGImage:[[UIImage imageNamed:path] CGImage]]];
        
        if (features != nil && features.count > 0) {
            for (CIQRCodeFeature* qrFeature in features) {
                NSString* message = qrFeature.messageString;
                
                return cStringCopy([message UTF8String]);
            }
        }
        
        return cStringCopy([@"" UTF8String]);
    }
}
