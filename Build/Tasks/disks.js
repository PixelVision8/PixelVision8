const gulp = require('gulp');
const zip = require('gulp-zip');

gulp.task('release-api-examples-disk', function(){

    return gulp.src("./Disks/APIExamples/**/*")
    .pipe(zip("APIExamples.pv8"))
    .pipe(gulp.dest(process.env.FINAL + "Disks/"));
  
  })

gulp.task('release-art-pack-disk', function(){

  return gulp.src("./Disks/ArtPacks/**/*")
  .pipe(zip("ArtPacks.pv8"))
  .pipe(gulp.dest(process.env.FINAL + "Disks/"));

})

gulp.task('release-games-disk', function(){

  return gulp.src("./Disks/Games/**/*")
  .pipe(zip("Games.pv8"))
  .pipe(gulp.dest(process.env.FINAL + "Disks/"));

})

gulp.task('release-demoscene-disk', function(){

  return gulp.src("./Disks/Demoscene/**/*")
  .pipe(zip("Demoscene.pv8"))
  .pipe(gulp.dest(process.env.FINAL + "Disks/"));

})