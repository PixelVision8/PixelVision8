const gulp = require('gulp');
const markdownpdf = require("markdown-pdf");

const path = require('path'),
    through = require('through2'),
    cheerio = require('cheerio');

var preProcessHtml = function(basePath) {
    return function() {
        return through(function(chunk, encoding, callback) {
            var $ = cheerio.load(chunk);

            $('img[src]').each(function() {
                var imagePath = $(this).attr('src');
                imagePath = path.resolve(basePath, imagePath);

                console.log("imagePath", imagePath);

                $(this).attr('src', 'file://' + (process.platform === 'win32' ? '/' : '') + imagePath);
            });

            this.push($.html());
            callback();
        });
    }
};

var mdDocs = [
    "../Docs/Home.md", 
    "../Docs/BackgroundColor.md",
    "../Docs/Booting-From-A-Disk.md",
    "../Docs/Button.md",
    "../Docs/C-Sharp-vs-Lua.md",
    "../Docs/CalculateDistance.md",
    "../Docs/CalculateIndex.md",
    "../Docs/CalculatePosition.md",
];

var bookPath = "../Releases/Docs/guide.pdf";

gulp.task('generate-pdf' ,function(cb)
{
    var options = {
        preProcessHtml: preProcessHtml(process.cwd().replace(".gulp", "Docs")),
    
        remarkable: {
            html: true,
            breaks: true,
            // plugins: [ require('remarkable-classy') ],
            syntax: [ 'footnote', 'sup', 'sub' ]
        }
    };

    markdownpdf(options).concat.from(mdDocs).to(bookPath, function () {
    console.log("Created", bookPath)
    })

    cb();
    
});
