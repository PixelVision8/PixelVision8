// Creates a dotnet template project

const gulp = require('gulp');
const del = require('del');
const fs = require('fs');

let dest = process.env.FINAL + 'DotnetTemplate'
let src = './Build/Templates/Dotnet/'

// Delete the old project
gulp.task('dotnet-clean', function () {
    return del(
      [dest], 
      {force: true}
    );
  })

gulp.task('dotnet-template-config', function() {
    return gulp.src("./Build/Templates/Dotnet/.template.config/**/*")
        .pipe(gulp.dest("Releases/DotnetTemplate/.template.config"));
    }
);

gulp.task('dotnet-tasks', function() {
    return gulp.src("./Build/Templates/Dotnet/.vscode/**/*")
        .pipe(gulp.dest("Releases/DotnetTemplate/.vscode"));
    }
);

gulp.task('dotnet-copy-contents', function() {
    return gulp.src(["Content/**/*","!Content/*.json"])
        .pipe(gulp.dest("Releases/DotnetTemplate/App/Content"));
    }
);

gulp.task('dotnet-copy-game', function() {
    return gulp.src("./Build/Templates/EmptyDisk/**/*")
        .pipe(gulp.dest("Releases/DotnetTemplate/Game"));
    }
);

gulp.task('dotnet-copy-program', function() {
    return gulp.src("./Runners/PixelVision8/Program.cs")
        .pipe(gulp.dest("Releases/DotnetTemplate/App"));
    }
);

gulp.task('dotnet-copy-files', function() {
    return gulp.src(['./Build/Templates/Dotnet/**/*', '.gitignore', 'gulpfile.js', 'package.json', './Build/Templates/Dotnet/README.md', 'LICENSE.txt'])
        .pipe(gulp.dest("Releases/DotnetTemplate"));
    }
);

gulp.task('dotnet-build-files', function() {
    return gulp.src(['./Build/Tasks/platforms/osx-x64.js', './Build/Tasks/build.js', './Build/Tasks/release.js'])
        .pipe(gulp.dest("Releases/DotnetTemplate/Build/Tasks"));
    }
);

gulp.task('dotnet-copy-dlls', function() {
    return gulp.src(['./Runners/PixelVision8/bin/Debug/Pixel Vision 8.dll'])
        .pipe(gulp.dest("Releases/DotnetTemplate/App/libs"));
    }
);

gulp.task("build-dotnet-template", gulp.series('dotnet-template-config', 'dotnet-tasks', 'dotnet-clean','dotnet-copy-files', 'dotnet-copy-contents', 'dotnet-copy-game', 'dotnet-build-files', 'build-desktop-runner-task', 'dotnet-copy-dlls', 'dotnet-copy-program'));

/*
DotnetTemplate
  -.vscode
  - App
  - Build
  - Game
  .gitignore
  gulpfile.js
  package.json
  README.md
  LICENSE.txt
*/