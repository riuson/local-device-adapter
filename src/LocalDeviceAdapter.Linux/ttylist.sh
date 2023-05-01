#!/bin/sh

# name:         COM4
# description:  Устройство с последовательным интерфейсом USB
# friendlyName: Устройство с последовательным интерфейсом USB (COM4)
# vendorId:     1054
# productId:    12920

# List of ttyusb's dev files one per line.
devs=$(ls -a1 /sys/class/tty/ttyUSB*/dev)

# Iterate Major/Minor nodes.
for dev in $devs
do
    # Get Major/Minor node numbers.
    mmn=$(cat $dev)
    major=$(echo "$mmn" | cut -d ":" -f 1)
    minor=$(echo "$mmn" | cut -d ":" -f 2)

    # Get path to device.
    path=$(udevadm info -rq name /sys/dev/char/$major:$minor)

    # Get device info.
    info=$(udevadm info --query=property --name=$path -x -P SP_ )
    vid=$(echo "$info" | grep -oP "(?<=SP_ID_VENDOR_ID=')[0-9a-fA-F]+(?=')")
    pid=$(echo "$info" | grep -oP "(?<=SP_ID_MODEL_ID=')[0-9a-fA-F]+(?=')")
    model=$(echo "$info" | grep -oP "(?<=^SP_ID_MODEL_ENC=').+(?='$)")

    # Write out info.
    echo "name:\t$path"
    echo "description:\t$model"
    echo "friendlyName:\t$model ($path)"
    printf "vendorId:\t%d\n" 0x$vid
    printf "productId:\t%d\n" 0x$pid
done

# name:   /dev/ttyUSB0
# description:    CP2102\x20USB\x20to\x20UART\x20Bridge\x20Controller
# friendlyName:   CP2102\x20USB\x20to\x20UART\x20Bridge\x20Controller (/dev/ttyUSB0)
# vendorId:       4292
# productId:      60000
# ............