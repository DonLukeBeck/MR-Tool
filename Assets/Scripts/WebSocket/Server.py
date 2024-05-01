import UdpComms as U
from Assets.Scripts.WebSocket.ImageAssembler import ImageAssembler

# Initialize the ImageAssembler
assembler = ImageAssembler()

# Create UDP socket to use for sending (and receiving)
sock = U.UdpComms(udpIP="127.0.0.1", portTX=8000, portRX=8001, enableRX=True, suppressWarnings=True)

while True:

    data = sock.ReadReceivedData()  # read data
    if data is not None:  # if NEW data has been received since last ReadReceivedData function call
        # Check if received data is an image chunk
        if data.startswith("ImageChunks") or data.startswith("Base64EncodedChunk"):
            assembler.process_image_data(data)
        # Check if received data is a question
        elif data.startswith("Question"):
            sock.SendData("I am unable to determine your current step, please provide a picture of the model by "
                          "looking towards it and pressing the \"Send Picture\" button")  # send response
            print(data)
        elif data.startswith("Step"):
            step_number = int(data.split(' ')[1])
            print(f"Step number: {step_number}")
        else:
            print(data)  # print new received data
