const shell = require('gulp-shell')
const gulp = require('gulp');

// Set the path to the .csproj file
process.env.PROJECT = "./Runners/Desktop/PixelVision8.DesktopRunner.csproj"

let options = ' -p:SelfContained=false -p:PublishSingleFile=false -p:GenerateFullPaths=true'

// The default task just build the game locally which you can launch and debug
gulp.task('build-desktop-runner-task', gulp.series([shell.task('dotnet build ' + process.env.PROJECT + options)]))

gulp.task(
    'default', 
    gulp.series( ['build-desktop-runner-task'] )
  );
  
let project = "./Runners/Desktop/PixelVision8.CSharpRunner.csproj"

// The default task just build the game locally which you can launch and debug
gulp.task('build-csharp-runner-task', gulp.series([shell.task('dotnet build ' + project + options)]))

project = "./Runners/LuaRunner/PixelVision8.LuaRunner.csproj"

// The default task just build the game locally which you can launch and debug
gulp.task('build-lua-runner-task', gulp.series([shell.task('dotnet build ' + project + options)]))

project = "./Runners/RoslynRunner/PixelVision8.RoslynRunner.csproj"

// The default task just build the game locally which you can launch and debug
gulp.task('build-roslyn-runner-task', gulp.series([shell.task('dotnet build ' + project + options)]))