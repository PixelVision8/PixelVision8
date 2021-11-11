const shell = require('gulp-shell')
const gulp = require('gulp');

// Set the path to the .csproj file
process.env.PROJECT = "./App/MyGame.DesktopRunner.csproj"

let options = ' -p:SelfContained=false -p:PublishSingleFile=false -p:GenerateFullPaths=true'

// The default task just build the game locally which you can launch and debug
gulp.task('build-desktop-runner-task', gulp.series([shell.task('dotnet build ' + process.env.PROJECT + options)]))

gulp.task(
    'default', 
    gulp.series( ['build-desktop-runner-task'] )
  );