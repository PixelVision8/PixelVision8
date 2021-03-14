const gulp = require('gulp');
let {run} = require('gulp-dotnet-cli');

gulp.task('build-run', ()=>{
  return gulp.src(process.env.PROJECT, {read: false})
      .pipe(run({
        additionalArgs: [process.env.DISKS + 'PixelVisionOS/', process.env.DISKS + 'APIExamples/'], // ['-d', 'Disks/PixelVisionOS/', '-disk', 'Disks/APIExamples/']
        echo: true
    }));
});

gulp.task('build-run-lite', ()=>{
  return gulp.src(process.env.PROJECT, {read: false})
      .pipe(run({
        additionalArgs: [],
        echo: true
    }));
});

gulp.task("run", gulp.series('build-run'));

gulp.task("run-lite", gulp.series('build-run-lite'));