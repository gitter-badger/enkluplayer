#include "stdlib.h"
#include <AVFoundation/AVFoundation.h>
#import <UIKit/UIKit.h>

extern "C"
{
    bool unity_hasCameraPermissions()
    {
        AVAuthorizationStatus authStatus = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];

        return authStatus == AVAuthorizationStatusAuthorized;
    }

    bool unity_hasDeniedCameraPermissions()
    {
        AVAuthorizationStatus authStatus = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];

        return authStatus == AVAuthorizationStatusDenied;
    }

    void unity_requestCameraPermissions()
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

    void unity_openSettings()
    {
        [[UIApplication sharedApplication] openURL:[NSURL URLWithString:UIApplicationOpenSettingsURLString]];
    }
    
    void unity_hasPhotosPermissions()
    {
        PHAuthorizationStatus status = [PHPhotoLibrary authorizationStatus];
        return authStatus == PHAuthorizationStatusAuthorized;
    }
    
    void unity_hasDeniedPhotosPermissions()
    {
        PHAuthorizationStatus status = [PHPhotoLibrary authorizationStatus];
        return authStatus == PHAuthorizationStatusDenied;
    }
    
    void unity_requestPhotosPermissions()
    {
        [PHPhotoLibrary requestAuthorization:^(PHAuthorizationStatus status) {
            if (status == PHAuthorizationStatusAuthorized)
            {
                NSLog("@Photo library access granted.");
            }
            else
            {
                NSLog(@"Photo library access denied.");
            }
        }]
    }
}
