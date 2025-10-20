#!/bin/bash

# Exit immediately if a command exits with a non-zero status.
set -e

# --- Argument Parsing ---
REBUILD=false
TAG=""

# Parse arguments
for arg in "$@"
do
    case $arg in
        --rebuild)
        REBUILD=true
        shift # Remove --rebuild from processing
        ;;
        *)
        TAG="$arg"
        ;;
    esac
done

# Check if a tag version is provided
if [ -z "$TAG" ]; then
  echo "Error: No tag version provided."
  echo "Usage: ./newtag.sh [--rebuild] <version>"
  exit 1
fi

# --- Pre-flight Checks ---

# Stash uncommitted changes if they exist
STASHED=false
if ! git diff-index --quiet HEAD --; then
    echo "Uncommitted changes detected. Stashing them automatically."
    git stash push -m "Auto-stash before tagging $TAG"
    STASHED=true
fi

# Check for unpushed commits
if [ -n "$(git log @{u}..)" ]; then
    echo "Error: There are unpushed commits. Please push them before creating a new tag."
    # If we stashed changes, pop them before exiting
    if [ "$STASHED" = true ]; then
        git stash pop
    fi
    exit 1
fi

# --- Tagging Logic ---

# If --rebuild is used, delete the existing tag first
if [ "$REBUILD" = true ]; then
    echo "Rebuild flag detected. Deleting existing tag '$TAG'."
    # Delete local tag (ignore error if it doesn't exist)
    git tag -d "$TAG" || true
    # Delete remote tag (ignore error if it doesn't exist)
    git push origin --delete "$TAG" || true
    echo "Existing tag '$TAG' deleted locally and remotely."
fi

echo "Creating new tag: $TAG"
git tag "$TAG"

echo "Pushing new tag: $TAG"
git push origin "$TAG"

# --- Cleanup ---

# If we stashed changes, pop them back
if [ "$STASHED" = true ]; then
    echo "Popping stashed changes."
    git stash pop
fi

echo "Done! Tag $TAG has been successfully created and pushed."
