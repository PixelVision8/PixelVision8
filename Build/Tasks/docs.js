const gulp = require('gulp');
const markdownPdf = require('gulp-markdown-pdf');
const replace = require('gulp-replace');
const path = require("path"); 
const fs = require('fs');



gulp.task('docs-concatenate-markdown', function(cb){

    var rootPath = path.resolve('Docs/images/') + "/"

    var toc = fs.readFileSync("Docs/_Sidebar.md", "utf8");

    var lines = toc.split("\n"); 

    lines.splice(1, 0, "* [About](Home)");

    var markdown = "";

    for (let i = 0; i < lines.length; i++) {
        
        const line = lines[i];
        
        if(line.substring(0,1) == "#")
        {
            markdown += line
        }
        else if(line.substring(0,1) == "*")
        {

            let split = line.split("](");

            markdown += "## " + split[0].substring(3) + "\n";
            
            let text = fs.readFileSync("Docs/" + split[1].substring(0, split[1].length-1) + ".md", "utf8")
            
            markdown += text.replace(/\]\(images\//g, "](" + rootPath);;
        }

        markdown += "\n";

    }

    fs.writeFile('Releases/Source/docs.md', markdown, cb);
});

gulp.task('docs-build-pdf', function(){

    var tmpDoc = 'Releases/Source/docs.md'

    return gulp.src(tmpDoc)
    .pipe(markdownPdf())
    .pipe(gulp.dest('Releases/Final/'));
  
  })

  gulp.task("build-docs", gulp.series('docs-concatenate-markdown', 'docs-build-pdf'));