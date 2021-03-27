const gulp = require('gulp');
const del = require('del');
const fs = require('fs');
const zip = require('gulp-zip');
const { Console } = require('console');

var sources = {

    "osx-x64" : ["osx-x64", "unix", "shared"],
    "win-x64" : ["win-x64", "shared"],
    "linux-x64" : ["linux-x64", "unix", "shared"],
    "linux-arm64" : ["linux-arm64"],
    "unity" : ["unity"]

}

gulp.task('release-clean', function () {
    return del(
      [process.env.FINAL + process.env.CURRENT_PLATFORM, zipName( process.env.FINAL + process.env.CURRENT_PLATFORM)], 
      {force: true}
    );
  });

gulp.task('release-platform', function(cb) {
    
    // Parse the paths
    var paths = sources[process.env.CURRENT_PLATFORM];
    
    var fileList = [];

    paths.forEach(path => {

        fileList.push(process.env.STAGING + 'Runner/' + path + "/**/*");

    });

    var dest = process.env.FINAL + process.env.CURRENT_PLATFORM;

    if(process.env.CURRENT_PLATFORM == "osx-x64")
    {
        dest += "/" + process.env.APP_NAME + ".app/Contents/MacOS/"
    }

    return gulp.src(fileList)
    .pipe(gulp.dest(dest));

    }
  );

gulp.task('release-runner', function(){

    var src =  process.env.STAGING + "Runner";

    var dest = process.env.FINAL + process.env.CURRENT_PLATFORM;

    if(process.env.CURRENT_PLATFORM == "osx-x64")
    {
        dest += "/" + process.env.APP_NAME + ".app/Contents/MacOS/"
    }

    return gulp.src(src + "/**/*", {ignore: process.env.STAGING + "/shared/Content/**/*"})
    .pipe(gulp.dest(dest + "/Content/PixelVisionOS/Runners/"));

})

function zipName(platform)
{
  
  var nameSplit = platform.split("-")[0];

  if(platform == 'osx-x64')
  {
    nameSplit = "macOS"
  }
  else if(platform == 'linux-arm64')
  {
    nameSplit = 'Linux Arm64'
  }
  else
  {
    nameSplit.charAt(0).toUpperCase() + nameSplit.slice(1);
  }

  return 'PixelVision8-'+nameSplit +'.zip';

}

gulp.task('release-zip', function(){

  var dest = process.env.FINAL + process.env.CURRENT_PLATFORM;

  return gulp.src(dest + "/**/*")
  .pipe(zip(zipName(process.env.CURRENT_PLATFORM)))
  .pipe(gulp.dest(process.env.FINAL));

})



gulp.task("release", gulp.series('build-next-platform', 'release-clean', 'release-platform'));//, 'release-runner'));