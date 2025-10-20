#!/bin/bash

# Exit immediately if a command exits with a non-zero status.
set -e

# Check if a tag version is provided
if [ -z "$1" ]; then
  echo "Error: No tag version provided."
  echo "Usage: ./newtag.sh <version>"
  exit 1
fi

TAG=$1

# Check for uncommitted changes and stash them if they exist
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

echo "Creating new tag: $TAG"
git tag "$TAG"

echo "Pushing new tag: $TAG"
git push origin "$TAG"

# If we stashed changes, pop them back
if [ "$STASHED" = true ]; then
    echo "Popping stashed changes."
    git stash pop
fi

echo "Done! Tag $TAG has been created and pushed."
