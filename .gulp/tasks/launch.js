const gulp = require('gulp');
let {run} = require('gulp-dotnet-cli');

gulp.task('build-run', ()=>{
  return gulp.src(process.env.PROJECT, {read: false})
      .pipe(run({
        additionalArgs: ['Disks/PixelVisionOS/'],
        echo: true
    }));
});

gulp.task("run", gulp.series('build-run'));