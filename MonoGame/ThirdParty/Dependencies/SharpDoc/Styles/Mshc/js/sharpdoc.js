// Copyright (c) 2010-2013 SharpDoc - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.       
// -------------------------------------------------------------------------------
// SplitPane handling Toc and Conent
// -------------------------------------------------------------------------------

function InstallCodeTabs() {
    var groupTabs = $$('.grouptab');
    groupTabs.each(function (groupTab, groupIndex) {
        var tabs = groupTab.getChildren('.tabs li.tab');
        var content = groupTab.getChildren('.tabcontent');
        tabs.each(function (tab, index) {
            tab.addEvent('click', function () {
                tabs.removeClass('selected');
                content.removeClass('selected');
                tabs[index].addClass('selected');
                content[index].addClass('selected');
            });
        });    
    });  
}

function SplitPane(splitPaneId, splitPaneToggleId, splitPaneResizerId)
{
    // does nothing in mshc
}  