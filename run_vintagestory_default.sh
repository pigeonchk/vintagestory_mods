#!/bin/bash

SOURCE=${BASH_SOURCE[0]}
while [ -L "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
  DIR=$( cd -P "$( dirname "$SOURCE" )" >/dev/null 2>&1 && pwd )
  SOURCE=$(readlink "$SOURCE")
  [[ $SOURCE != /* ]] && SOURCE=$DIR/$SOURCE
done
DIR=$( cd -P "$( dirname "$SOURCE" )" >/dev/null 2>&1 && pwd )

if [[ -z "${DOTNET_PROJECT_NAME}" ]] then
    echo "Set DOTNET_PROJECT_NAME=<project_name> before calling this script"
    exit 1
fi

if [[ -z "${VINTAGE_STORY}" ]] then
    echo "Set VINTAGE_STORY=<vintagestory_path> before calling this script"
    exit 1
fi

DATAPATH="/${VINTAGE_STORY}/${DOTNET_PROJECT_NAME}_mod_dev_VintagestoryData"
MODPATH="${DIR}/Output/${DOTNET_PROJECT_NAME}"

ARGS="--dataPath=${DATAPATH} --tracelog --addModPath=${MODPATH}"

env mesa_glthread=true gamemoderun "${VINTAGE_STORY}/Vintagestory" ${ARGS}
