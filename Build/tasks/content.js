const gulp = require('gulp');
const del = require('del');
const merge = require('gulp-merge-json');

var contentPath = process.env.STAGING + 'Runner/shared/Content/';

gulp.task('content-clean', function () {
    return del(
      [contentPath], 
      {force: true}
    );
  });

// Copy over the Pixel Vision OS Disk System folder to the Content/PixelVisionOS/ folder
gulp.task('content-os', function() {
  
    console.log("Copying over Pixel Vision OS")
  
    return gulp.src(["./Disks/PixelVisionOS/System/**/*", "./Content/PixelVisionOS/**/*"])
        .pipe(gulp.dest(contentPath+'/PixelVisionOS/'));
    }

);

// Copy over the Pixel Vision OS Disk System folder to the Content/PixelVisionOS/ folder
gulp.task('content-effects', function() {
  
  console.log("Copying over effects")

  return gulp.src("./Content/Effects/**/*")
      .pipe(gulp.dest(contentPath+'/Effects/'));
  }

);

// Copy over the Pixel Vision OS Disk System folder to the Content/PixelVisionOS/ folder
gulp.task('content-fonts', function() {
  
  console.log("Copying over fonts")

  return gulp.src("./Content/Fonts/**/*")
      .pipe(gulp.dest(contentPath+'/Fonts/'));
  }

);

gulp.task('content-bios', function() {
  console.log("Copying over bios")
  return gulp.src(['./Content/bios.json', process.env.SCRIPTS + 'Templates/bios-template.json'])
    .pipe(merge({fileName: 'bios.json'}))
    .pipe(gulp.dest(contentPath));
  }
);

gulp.task("content", gulp.series('content-clean', 'content-os', 'content-effects', 'content-fonts', 'content-bios'));