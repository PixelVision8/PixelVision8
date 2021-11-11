const gulp = require('gulp');
const requireDir  = require('require-dir');
const fs = require('fs');
requireDir('./Build/Tasks/', { recurse: true });

var xpath = require('xpath')
  , dom = require('xmldom').DOMParser

process.env.RELEASE = "./Releases/"

// Set the path to the .csproj file
process.env.STAGING = process.env.RELEASE  + "Source/"

// Set the path to the .csproj file
process.env.FINAL = process.env.RELEASE  + "Final/"

var xml = fs.readFileSync(process.env.PROJECT, "utf8");
var xmlDoc = new dom().parseFromString(xml)

process.env.APP_NAME = xpath.select("string(//AssemblyName)", xmlDoc);
process.env.NAME_SPACE = xpath.select("string(//RootNamespace)", xmlDoc)
process.env.VERSION = xpath.select("string(//Version)", xmlDoc)

process.env.PLATFORMS = "osx-x64,win-x64,linux-x64";
process.env.CURRENT_PLATFORM = "";
process.env.BUILD_PATH = "";
process.env.SCRIPTS = "./Build/"

// Create the first round of tasks based on the platform list
var tasks = [];

for (let index = 0; index < process.env.PLATFORMS.split(",").length; index++) {
  tasks.push('build');

  if(index == 0) {
      tasks.push('mac-bundle');
  }

  tasks.push('release');

}

// Perform all of the builds and packages up each exe
gulp.task(
  'package', 
  gulp.series( tasks )
);