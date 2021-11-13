const gulp = require('gulp');
const del = require('del');
let {publish} = require('gulp-dotnet-cli');

gulp.task('build-next-platform', function(callback)
{
  // Get the list of platforms
  var platforms = process.env.PLATFORMS.split(",");

  index = platforms.indexOf(process.env.CURRENT_PLATFORM) + 1;

  // Save the value for the next run
  process.env.CURRENT_PLATFORM = platforms[index]
  process.env.BUILD_PATH = process.env.STAGING + process.env.CURRENT_PLATFORM + "/";

  console.log("Preparing to build", process.env.CURRENT_PLATFORM, "at", process.env.BUILD_PATH);

  callback();
});

gulp.task('build-clean', function () {
  return del(
    [process.env.BUILD_PATH], 
    {force: true}
  );
})

// TODO need to modify the bios to load the game

gulp.task('build-publish', ()=>{
  return gulp.src(process.env.PROJECT, {read: false})
              .pipe(publish({configuration: 'Release', runtime: process.env.CURRENT_PLATFORM, output: process.env.BUILD_PATH}));
});

gulp.task('post-build-clean', function () {

  var files = [
    process.env.BUILD_PATH + "*.bak",
    process.env.BUILD_PATH + "*.pdb",
  ];

  return del(
    files, 
    {force: true}
  );

});

gulp.task("build", gulp.series('build-next-platform', 'build-clean', 'build-publish', 'post-build-clean'));