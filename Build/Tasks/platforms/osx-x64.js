const gulp = require('gulp');
const rename = require('gulp-rename');
const replace = require('gulp-string-replace');

gulp.task('mac-icon', function() {
    return gulp.src("./Projects/PixelVision8/Icon.icns")
        .pipe(rename('App.icns'))
        .pipe(gulp.dest(process.env.FINAL + "/osx-x64/" + process.env.APP_NAME + ".app/Contents/Resources/"));
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
    .pipe(gulp.dest(process.env.FINAL + "/osx-x64/" + process.env.APP_NAME + ".app/Contents/"))
});

gulp.task('mac-init', function(cb)
{
    process.env.CURRENT_PLATFORM = "osx-x64";

    cb();

});
