const gulp = require('gulp');
const rename = require('gulp-rename');

gulp.task('linux64-launcher', function() {
    return gulp.src(process.env.SCRIPTS + "Templates/Linux64 Launcher")
        .pipe(rename('Pixel Vision 8'))
        .pipe(gulp.dest(process.env.FINAL + "/linux-x64/"));
    }
  );