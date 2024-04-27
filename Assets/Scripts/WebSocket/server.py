import UdpComms as U
import time

# Create UDP socket to use for sending (and receiving)
sock = U.UdpComms(udpIP="127.0.0.1", portTX=8000, portRX=8001, enableRX=True, suppressWarnings=True)

i = 0

while True:
    sock.SendData('Sent from Python: ' + str(i)) # Send this string to other application
    i += 1
    # I am unable to determine your current step, please provide a picture of the model by looking towards it and pressing the "Send Picture" button
    data = sock.ReadReceivedData() # read data

    if data != None: # if NEW data has been received since last ReadReceivedData function call
        print(data) # print new received data

    time.sleep(1)