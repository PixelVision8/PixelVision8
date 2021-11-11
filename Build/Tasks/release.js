const gulp = require('gulp');
const del = require('del');
const zip = require('gulp-zip');

gulp.task('release-clean', function () {
    return del(
      [process.env.FINAL + process.env.CURRENT_PLATFORM, zipName( process.env.FINAL + process.env.CURRENT_PLATFORM)], 
      {force: true}
    );
  });

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

<<<<<<< HEAD
  return (process.env.APP_NAME.split(' ').join('-')+"-"+nameSplit +'.zip'.toLowerCase());
=======
  return process.env.APP_NAME.toLowerCase().split(' ').join('-')+"-"+nameSplit +'.zip';
>>>>>>> f3ac589084992393023bc942320c0b750a2c4435

}

gulp.task('release-zip', function(){

  var dest = process.env.STAGING + process.env.CURRENT_PLATFORM;

  return gulp.src(dest + "/**/*")
  .pipe(zip(zipName(process.env.CURRENT_PLATFORM)))
  .pipe(gulp.dest(process.env.FINAL));

})



gulp.task("release", gulp.series('release-clean', 'release-zip'));