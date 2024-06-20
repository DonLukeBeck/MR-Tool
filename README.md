# Mixed Reality Tool developed for the Master Thesis experiment

The project has been built on top of the [Mixed Reality Template](https://docs.unity3d.com/Packages/com.unity.template.mixed-reality@1.0/manual/index.html) provided by Unity when setting up a new project. The template already has preinstalled the packages needed to support development on OpenXR platforms, as well as gesture interactions and standardized UI elements. In addition, the Affordance system provides feedback for the user with visual and auditory cues. This requires the use of the XR Interactable Affordance State Provider with a specified interactable source. For the interactables included in the MRTool scene, there is an Audio Affordance Receiver and a Color Affordance Receiver already set up.

# Running
* Download or clone the project

* To incorporate the latest features of Unity such as Unity Sentis, which allows using built-in AI models at runtime without the need for API calls, the project is currently using Unity 6 Beta version 6000.0.0b16. Apart from the packages from the Mixed Reality Template, the following packages and files need to be downloaded and installed:
   -  Install package com.unity.nuget.newtonsoft-json from Package Manger if not already installed
   -  Install package com.unity.sentis from Package Manager if not already installed
   -  For the Speech-To-Text model to work, go to https://huggingface.co/unity/sentis-whisper-tiny/tree/main and download audio-decoder-tiny.sentis, audio-encoder-tiny.sentis, logmel-spectro.sentis and vocab.json
   -  For the Text-To-Speech model to work, go to https://huggingface.co/unity/sentis-whisper-tiny/tree/main and download jets-text-to-speech.sentis and phoneme_dict.txt

 * In the Assets folder create a subfolder StreamingAssets (the path should be Assets/StreamingAssets) and add the following within it:
    - AudioDecoder_Tiny.sentis
    - AudioEncoder_Tiny.sentis
    - LogMelSepctro.sentis
    - vocab.json
    - jets-text-to-speech.sentis
    - phoneme_dict.txt

  * Open the MRTool scene

  * Run Server.py

  * Connect your headset to the computer and press Play within the Unity Editor

# Implementation details and project structure

The spatial UI includes a coaching UI and video player for onboarding users into the MR application. The Coaching UI GameObject is controlled by the Goal Manager located in the MR Interaction Setup. The Goal Manager controls the progression of content within the UI, turning on/off related GameObjects, as well as altering the Lazy Follow behavior of the UI depending on the instructions in the step. The Tutorial Video Panel game object within the UI displays a video player to communicate basic input mapping for the MRTool scene. Users have the ability to move the canvas in space by grabbing the canvas by either the header or handle at the bottom of the canvas. Bill-boarding with the Lazy Follow component is on the prefab by default, with the positional transformation of the canvas being determined by the direct/ray interaction. This functionality is similarly used in the Interactive Menu Manipulator and Helper Menu Manipulator game objects, which give the user the ability to interact with the model as well as the dialogue agent.

The following paths contain the most important documents and functionality of the tool:
 * Assets/MRTemplateAssets/Scripts/GoalManager.cs is the utility class that controls most of the application:
     - The Start() function initializes the Onboarding Goals, the text, and the visibility of UI elements and buttons, as well as the tutorial videos.
     - The NextStep() and PreviousStep() functions handle the progression of the 3D Model through the steps, update the elements, and perform the color animations.
     - The AskQuestion() method enables the functionality for recording the user's question via the microphone and then sending it to the AI model for transcription.
     - The SendPicture() method handles capturing the screen, breaking the image into chunks, and sending it over the WebSocket connection. The function also updates the UI to display the dialogue agent response.
     - The Restart() method restarts the assembly process and sends to the server the number of menu and model interactions.

 * Assets/Scripts/WebSocket/Images folder contains the images from the manual from both models. The first model images are named with the following structure step{step_number}.jpg while the second model is named step_{step_number}.jpg

 * Assets/Scripts/WebSocket/Manuals folder contains the PDF versions of the Lego set manuals used in the experiment 

 * Assets/Scripts/WebSocket/Server.py contains the script that continuously awaits data from the client. Depending on the type of data received, it delegates to appropriate handling functions:
   - For image data, it uses the ImageAssembler to handle the reconstruction of images transmitted in chunks over a network connection
   - For questions, it calls handle_question
   - For step instructions, it calls handle_step
   - For requests to resend images (sometimes packets get lost over the network), it sends the image data again

 * Assets/Scripts/WebSocket/UdpComms.py is the Python script that facilitates communication with the Unity application using UDP sockets. The SendData method is used to send image metadata and image chunks from the Python application to the Unity application. The ReadReceivedData method is used in the main loop to check if new data has been received. Depending on the type of data received (image chunk, question, step instruction), it delegates the handling to the appropriate method.

 * Assets/Scripts/WebSocket/UdpSocket.cs is the Unity script that establishes a UDP communication bridge with a Python server. It handles sending and receiving messages, processing received data, and updating the Unity UI accordingly. Similar to Server.py, it processes the input based on the type of data received, either images or text.

# Connecting your own server to the application and running the application standalone on a headset
Currently, the project is set to run within the Unity Editor, hence the Server is configured to run locally. This does not work however for running the application standalone on a headset. To achieve that, the following changes need to be made:

 * Within the UdpSocket game object script component change the IP, Rx Port, and Tx Port to the values of your server.
 * Change the sock variable on the Server.py to the IP of the headset, while the Rx Port and Tx Port need to be the opposite values added previously to UdpSocket.
 * Instead of pressing play within the Unity Editor, follow the [Mixed Reality Template](https://docs.unity3d.com/Packages/com.unity.template.mixed-reality@1.0/manual/index.html) build instructions at the bottom of the page to deploy the app on your device.
 * Once the app is built, open it on the headset and make sure Server.py is running on your machine or a cloud server.

# Application walkthrough

At the beginning, the user selects the model he would like to assemble, as well as the microphone to be used during assembly. After this initial step, the user will see four instruction cards detailing the experiment setup and how to interact with the environment. Once the cards are either skipped or read using either gestures or controllers, the user will see the real world and his UI. Before starting the assembly, there are two tutorial videos explaining pinching and poking. To the left, the interactive menu can be found, while on the right lies the 3D Model. Further to the right there is another helper menu to be used just in case something does not go as intended. It contains a toggle for the tutorial videos, the onboarding cards, the progress bar, the passthrough (the switch between VR and MR), and a model finder (an arrow that points toward the 3D Model in case it gets lost during assembly). 

Once the user presses on the 'Start Assembly' button, the tutorial videos will disappear (if they were not already closed by the user) and a progress bar as well as a textual response of the dialogue agent will appear in their place. The 3D Model will now show the first piece that needs to be assembled. The user can press either the 'Next Step' or 'Previous Step' button to change the assembly step. The dialogue-agent interaction can be done with either the 'Ask Question' or 'Send Picture' button. Finally, the Restart button will reset the assembly to the beginning. For a visual walkthrough, the following video is available: [Multimodal Immersive Systems for Assembly in Mixed Reality](https://youtu.be/29t2pByljW8)
