const gulp = require('gulp');
const requireDir  = require('require-dir');
const fs = require('fs');
const libxmljs = require("libxmljs");
const { platform } = require('os');

// Set the path to the .csproj file
process.env.STAGING = "../Builds/"

// Set the path to the .csproj file
process.env.RELEASE = "../Releases/"

// Set the path to the .csproj file
process.env.PROJECT = "../PixelVision8.CoreDesktop.csproj"

var xml = fs.readFileSync(process.env.PROJECT, "utf8");
var xmlDoc = libxmljs.parseXml(xml);

process.env.APP_NAME = xmlDoc.get('//AssemblyName').text();
process.env.NAME_SPACE = xmlDoc.get('//RootNamespace').text();
process.env.VERSION = xmlDoc.get('//Version').text();

// Require all tasks.
var dir = requireDir('./tasks/', { recurse: true });

process.env.PLATFORMS = "osx-x64,win-x64,linux-x64";//,linux-arm64";
process.env.CURRENT_PLATFORM = "";
process.env.BUILD_PATH = "";


// Create the first round of tasks based on the platform list
var tasks = [];

for (let index = 0; index < process.env.PLATFORMS.split(",").length; index++) {
  tasks.push('build');
  tasks.push('runner');
}

gulp.task('runner-unix', function(cb)
    {

      process.env.CURRENT_PLATFORM = "unix";
      process.env.TARGET_PLATFORM = "linux-x64";
        // Switch to unix

        cb();

    }
);

gulp.task('runner-shared', function(cb)
    {
      process.env.CURRENT_PLATFORM = "shared";
      process.env.TARGET_PLATFORM = "osx-x64";
        // Switch to shared

        cb();

    }
)

gulp.task('reset-platforms', function(cb)
    {

      process.env.CURRENT_PLATFORM = "";

      cb();
    }
)

tasks.push(gulp.series('runner-unix', 'runner', 'runner-shared', 'runner'));

tasks.push('content');

tasks.push('reset-platforms');

for (let index = 0; index < process.env.PLATFORMS.split(",").length; index++) {
  
  tasks.push('release');

  if(index == 0) {
    tasks.push('mac-icon');
    tasks.push('mac-plist');
  }

  tasks.push('release-zip');

}

// Perform all of the builds
gulp.task(
  'default', 
  gulp.series( tasks )
);

// Perform all of the builds
gulp.task(
  'launch', 
  gulp.series( ['build-run'] )
);

// Perform all of the builds
gulp.task(
  'launch-lite', 
  gulp.series( ['build-run-lite'] )
);
