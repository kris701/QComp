<p align="center">
    <img src="https://github.com/kris701/QComp/assets/22596587/84c22fdc-7da6-438d-8339-576ae7309748" width="200" height="200" />
</p>

# Quick Compare - QComp
QComp is a Visual Studio Extension that allows you to quickly compare performance differnces between current and older versions of your project.

## How to use?
Firstly, install the extension by finding it on the [Visual Studio Markedplace](https://marketplace.visualstudio.com/items?itemName=KristianSkovJohansen.qcomp).
When you have installed it, you should be able to see a new tool window under View in Visual Studio:

![image](https://github.com/kris701/QComp/assets/22596587/69388321-9316-4ae6-9f06-9271cfe906c1)

If you click on this menu item, the QComp tool window will appear, looking like this:

![image](https://github.com/kris701/QComp/assets/22596587/ac3ca4d4-0b2b-4af4-8a5b-e24842f0d01b)

You can then save your current binary (both works in Debug and Release mode), by giving it a name and clicking the save button.
You should then be able to see the saved binary in the dropdown menu:

![image](https://github.com/kris701/QComp/assets/22596587/890ccf70-44a3-4187-9c16-5e4b46caab06)

If you then select that binary and clicks compare (you can also set different arguments and rounds under the `Arguments` menu), QComp will run both binaries in the background.
In the end, you are given a table with some general information, as well as a scatter plot of the runtime differences of the two binaries:

![image](https://github.com/kris701/QComp/assets/22596587/6273c343-059c-41b7-9331-44cea0a2278b)

And thats is! You can have as many binaries as you want saved, and compare them against you current build any time!