const gulp = require('gulp');
const rename = require('gulp-rename');
const replace = require('gulp-string-replace');
var fs = require('fs');

gulp.task('mac-bundle-start', function(done) {

    fs.rename(process.env.BUILD_PATH, process.env.STAGING + process.env.CURRENT_PLATFORM + "_Tmp", function (err) {
        if (err) {
          throw err;
        }
        done();
      });
    });

gulp.task('mac-bundle-finish', function(done) {

    fs.rename(process.env.STAGING + process.env.CURRENT_PLATFORM + "_Tmp", process.env.BUILD_PATH + process.env.APP_NAME + ".app/Contents/MacOS/", function (err) {
        if (err) {
          throw err;
        }
        done();
      });
    });

gulp.task('mac-icon', function() {
    return gulp.src("./Runners/Desktop/Icon.icns")
        .pipe(rename('App.icns'))
        .pipe(gulp.dest(process.env.BUILD_PATH + "/" + process.env.APP_NAME + ".app/Contents/Resources/"));
    }
  );
  
gulp.task('mac-plist', function() {
    
    var plistData = 
    "<key>CFBundleName</key>\
    <string>" + process.env.APP_NAME + "</string>\
    <key>CFBundleIdentifier</key>\
    <string>" + process.env.NAME_SPACE + "</string>\
    <key>CFBundleDisplayName</key>\
    <string>" + process.env.VERSION + "</string>\
    <key>CFBundleExecutable</key>\
    <string>" + process.env.APP_NAME + "</string>\
    <key>NSHumanReadableCopyright</key>\
    <string>Copyright © " + process.env.APP_NAME + " 2020</string>";

    return gulp.src([process.env.SCRIPTS + "Templates/Info.plist"])
    .pipe(replace('%appinfo%', plistData))
    .pipe(gulp.dest(process.env.BUILD_PATH + process.env.APP_NAME + ".app/Contents/"))
});

gulp.task("mac-bundle", gulp.series('mac-bundle-start', 'mac-icon', 'mac-plist', 'mac-bundle-finish'));
