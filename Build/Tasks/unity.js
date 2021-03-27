const gulp = require('gulp');
const rename = require('gulp-rename');
const replace = require('gulp-string-replace');
const fs = require('fs');
const zip = require('gulp-zip');
const del = require('del');
var path = require('path');

var unityFiles  = process.env.SCRIPTS + 'Templates/runner-files.json'
var templatePath  = process.env.SCRIPTS + "Templates/UnityRunner/**/*"
var templateStaging  = process.env.STAGING + "UnityRunner/"
var finalPath  = process.env.FINAL + "UnityRunner/"
var codeDestination  = "./" + templateStaging + "Assets/"

gulp.task('unity-clean', function () {
    return del(
      [templateStaging], 
      {force: true}
    );
  });

gulp.task('unity-template', function(cb) {
    
    return gulp.src(templatePath, {'allowEmpty':true})
    .pipe(gulp.dest(templateStaging));

    }

);

gulp.task('unity-copy-code', function(cb) {
    
    // Read the file list
    var json = fs.readFileSync(unityFiles);

    // Parse the files
    var files = JSON.parse(json)["unity"];

    if(files.length == 0){
        return cb();
    }    

    return gulp.src(files, {
        base: './', allowEmpty: true
        }).pipe(gulp.dest(codeDestination));
    
    }

);

gulp.task('unity-zip', function(){

    var dest = process.env.FINAL + "UnityRunner";
  
    return gulp.src(templateStaging + "/**/*")
    .pipe(zip("UnityRunner.zip"))
    .pipe(gulp.dest(process.env.FINAL));
  
  })

gulp.task("build-unity-project", gulp.series('unity-clean', 'unity-template', 'unity-copy-code', 'unity-zip'));