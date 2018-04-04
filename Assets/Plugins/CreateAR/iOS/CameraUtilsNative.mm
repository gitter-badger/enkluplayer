#include "stdlib.h"
#include <AVFoundation/AVFoundation.h>
#import <UIKit/UIKit.h>

extern "C" bool unity_hasCameraPermissions()
{
    AVAuthorizationStatus authStatus = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];

    return authStatus == AVAuthorizationStatusAuthorized;
}

extern "C" void unity_openSettings()
{
    [[UIApplication sharedApplication] openURL:[NSURL URLWithString:UIApplicationOpenSettingsURLString]];
}