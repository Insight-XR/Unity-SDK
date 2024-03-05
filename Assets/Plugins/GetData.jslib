mergeInto(LibraryManager.library, {
    GetCamData: function (filepath, objectname, callback, fallback, url) {
        try {
            console.log("setting pointers");
            var Pfilepath = Pointer_stringify(filepath);
            var Pobjectname = Pointer_stringify(objectname);
            var Pcallback = Pointer_stringify(callback);
            var Pfallback = Pointer_stringify(fallback);
            var Purl = Pointer_stringify(url);

            console.log("Getting request");
            var xhr = new XMLHttpRequest();

            xhr.onload = function () {
                if (xhr.status === 200) {
                    console.log("request was 200");
                    var jsonData = JSON.parse(xhr.responseText);
                    var jsonString = JSON.stringify(jsonData, null, 2);

                    console.log("Sending data back to C#");

                    // Call the function with the JSON content
                    window.unityInstance.SendMessage(Pobjectname, Pcallback, jsonString);

                    console.log('File content sent to ' + Pobjectname);
                } else {
                    console.error('Failed to retrieve file content from ' + Purl);
                }
            };

            xhr.open('GET', Purl, true);
            xhr.send();
        } catch (error) {
            console.error('Error:', error.message);
        }
    }
});