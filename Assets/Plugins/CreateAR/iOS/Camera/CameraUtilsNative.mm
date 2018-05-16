#include "stdlib.h"
#include <AVFoundation/AVFoundation.h>
#import <UIKit/UIKit.h>

extern "C" bool unity_hasCameraPermissions()
{
    AVAuthorizationStatus authStatus = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];

    return authStatus == AVAuthorizationStatusAuthorized;
}

extern "C" bool unity_hasDeniedCameraPermissions()
{
    AVAuthorizationStatus authStatus = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];

    return authStatus == AVAuthorizationStatusDenied;
}

extern "C" void unity_requestCameraPermissions()
{
    [AVCaptureDevice requestAccessForMediaType:AVMediaTypeVideo completionHandler:^(BOOL granted) {
        if (granted)
        {
            NSLog(@"Camera access granted.");
        }
        else
        {
            NSLog(@"Camera access denied.");
        }
    }];
}

extern "C" void unity_openSettings()
{
    [[UIApplication sharedApplication] openURL:[NSURL URLWithString:UIApplicationOpenSettingsURLString]];
}