#!/bin/sh

# name:         COM4
# description:  Устройство с последовательным интерфейсом USB
# friendlyName: Устройство с последовательным интерфейсом USB (COM4)
# vendorId:     1054
# productId:    12920

# List of ttyusb's dev files one per line.
devs=$(ls -a1 /sys/class/tty/ttyUSB*/dev)

echo "["
any=false

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
    model=$(echo "$info" | grep -oP "(?<=^SP_ID_MODEL=').+(?='$)")

    # Write out info.

    #[ ! -z "$infoString" ] && echo ","
    #infoString="  {\"name\":\"$path\",\"description\":\"$model\",\"friendlyName\":\"$model ($path)\",\"vendorId\":\"$vid\",\"productId\":\"$pid\"}"
    #echo -n $infoString

    if $any ; then
        echo ","
    fi
    echo "  {"
    echo "    \"name\": \"$path\","
    echo "    \"description\": \"$model\","
    echo "    \"friendlyName\": \"$model ($path)\","
    #echo "    \"vendorId\": \"0x$vid\","
    printf "    \"vendorId\": \"%d\",\n" 0x$vid
    #echo "    \"productId\": \"0x$pid\""
    printf "    \"productId\": \"%d\"\n" 0x$pid
    echo -n "  }"
    any=true
done

echo "\n]"

