#import <Foundation/Foundation.h>
#import <Photos/Photos.h>

@interface UNativeGallery:NSObject
+ (void)saveScreenshot:(NSString *)path;
+ (void)saveVideo:(NSString *)path;
@end

@implementation UNativeGallery

// Credit: http://www.techotopia.com/index.php/Accessing_the_iPhone_Camera_and_Photo_Library_(iOS_6)#Saving_Movies_and_Images
+ (void)saveScreenshot:(NSString *)path {
	UIImage *image = [UIImage imageWithContentsOfFile:path];
    UIImageWriteToSavedPhotosAlbum(image, self,
	   @selector(image:finishedSavingWithError:contextInfo:),
	   (__bridge_retained void *) path);
}

// Credit: http://www.techotopia.com/index.php/Accessing_the_iPhone_Camera_and_Photo_Library_(iOS_6)#Saving_Movies_and_Images
+ (void)saveVideo:(NSString *)path {
	if(UIVideoAtPathIsCompatibleWithSavedPhotosAlbum(path))
	{
		UISaveVideoAtPathToSavedPhotosAlbum(path, self,
          @selector(video:finishedSavingWithError:contextInfo:),
          (__bridge_retained void *) path);
	}
	else
	{
		[[NSFileManager defaultManager] removeItemAtPath:path error:nil];
	}
}

+ (void)image:(UIImage *)image finishedSavingWithError:(NSError *)error contextInfo:(void *)contextInfo {
    NSString* path = (__bridge_transfer NSString *)(contextInfo);
	[[NSFileManager defaultManager] removeItemAtPath:path error:nil];
    
    // find it!
    PHFetchOptions *fetchOptions = [PHFetchOptions new];
    fetchOptions.sortDescriptors = @[[NSSortDescriptor sortDescriptorWithKey:@"creationDate" ascending:NO]];
    PHAsset *asset = [PHAsset fetchAssetsWithMediaType:PHAssetMediaTypeImage options:fetchOptions].firstObject;
    
    if (asset != nil) {
        NSString *localIdentifier = asset.localIdentifier;
        NSString *urlString = [NSString stringWithFormat:@"instagram://library?LocalIdentifier=%@", localIdentifier];
        NSURL *instagramURL = [NSURL URLWithString:urlString];
        if ([[UIApplication sharedApplication] canOpenURL: instagramURL]) {
            [[UIApplication sharedApplication] openURL: instagramURL];
        }
    } else {
        NSLog(@"Could not find asset!");
    }
}

+ (void)video:(NSString *)videoPath finishedSavingWithError:(NSError *)error contextInfo:(void *)contextInfo {
    NSString* path = (__bridge_transfer NSString *)(contextInfo);
	[[NSFileManager defaultManager] removeItemAtPath:path error:nil];
}

@end


extern "C" void _ScreenshotWriteToAlbum(const char* path) {
	[UNativeGallery saveScreenshot:[NSString stringWithUTF8String:path]];
}

extern "C" void _VideoWriteToAlbum(const char* path) {
	[UNativeGallery saveVideo:[NSString stringWithUTF8String:path]];
}
