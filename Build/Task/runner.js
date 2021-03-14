const gulp = require('gulp');
const del = require('del');
const fs = require('fs');

var runnerPath = process.env.STAGING + 'Runner/';

function tokenReplace (text, tokens)
{
    for (var i = 0; i < tokens.length; i++) {
        text.replace(new RegExp(tokens[i], "g"),"{"+i+"}");
    }
    // string processing here
    for (var i = 0; i < tokens.length; i++) {
        text = text.replace(new RegExp("\\{"+i+"\\}","g"),tokens[i]);
    }

    return text;
}

gulp.task('runner-clean', function () {
    return del(
      [runnerPath + process.env.CURRENT_PLATFORM], 
      {force: true}
    );
  });

gulp.task('runner-platform', function(cb) {
    
    // Read the file list
    var json = fs.readFileSync(process.env.SCRIPTS + 'Templates/runner-files.json');

    // Parse the paths
    var paths = JSON.parse(json)[process.env.CURRENT_PLATFORM];
    
    if(paths.length == 0){
      return cb();
    }    

    var fileList = [];

    var targetPlatform = process.env.TARGET_PLATFORM == null ? process.env.CURRENT_PLATFORM : process.env.TARGET_PLATFORM;

    paths.forEach(path => {

        fileList.push(process.env.STAGING + tokenReplace(path, [targetPlatform, process.env.APP_NAME]));

    });

    return gulp.src(fileList, {'allowEmpty':true})
    .pipe(gulp.dest(runnerPath + process.env.CURRENT_PLATFORM));

    }
  );


gulp.task("runner", gulp.series('runner-clean', 'runner-platform'));