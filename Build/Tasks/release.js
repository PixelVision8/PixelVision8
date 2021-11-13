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

  return (process.env.APP_NAME.split(' ').join('-')+"-"+nameSplit +'.zip').toLowerCase();

}

gulp.task('release-zip', function(){

  var dest = process.env.STAGING + process.env.CURRENT_PLATFORM;

  return gulp.src(dest + "/**/*")
  .pipe(zip(zipName(process.env.CURRENT_PLATFORM)))
  .pipe(gulp.dest(process.env.FINAL));

})



gulp.task("release", gulp.series('release-clean', 'release-zip'));