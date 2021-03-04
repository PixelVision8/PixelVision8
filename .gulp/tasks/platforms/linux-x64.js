const gulp = require('gulp');
const rename = require('gulp-rename');

gulp.task('linux64-launcher', function() {
    return gulp.src("./templates/Linux64 Launcher")
        .pipe(rename('Pixel Vision 8'))
        .pipe(gulp.dest(process.env.RELEASE + "/linux-x64/"));
    }
  );