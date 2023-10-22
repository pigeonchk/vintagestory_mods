#!/bin/bash

DOTNET_PROJECT_NAME="$(basename *.csproj .csproj)"
VINTAGE_STORY="/mnt/games/vintagestory"

export DOTNET_PROJECT_NAME
export VINTAGE_STORY
