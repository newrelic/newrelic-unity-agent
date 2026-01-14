#!/bin/bash

# --- 1. REQUIRE VERSION ---
if [ -z "$PASSED_VERSION" ]; then
    echo "::error::PASSED_VERSION missing"
    exit 1
fi

VERSION=$PASSED_VERSION

# --- 2. PARSE CHANGELOG.MD ---
# 1. Finds the header matching '## VERSION'
# 2. Collects everything until it hits the NEXT '## ' followed by a number
# 3. This handles the double-hash format used in your Unity changelog
RELEASE_NOTES=$(awk "/^## ${VERSION}/{flag=1;next} /^## [0-9]/{flag=0} flag" CHANGELOG.md)

# Clean up: Remove empty lines and format bullets
# Ensures lines starting with '-' become '* '
IMPROVEMENTS=$(echo "$RELEASE_NOTES" | grep "^-" | sed 's/^- /* /')

# Fallback if no specific improvements found
if [ -z "$IMPROVEMENTS" ]; then
    IMPROVEMENTS="* Updated native agent dependencies."
fi

# --- 3. CREATE THE MDX ---
RELEASE_DATE=$(date +%Y-%m-%d)
# Unity uses GitHub releases as the primary distribution link
FINAL_DOWNLOAD_URL="${REPO_URL}/releases/tag/v${VERSION}"

cat > "release-notes.mdx" << EOF
---
subject: ${AGENT_TITLE}
releaseDate: '${RELEASE_DATE}'
version: ${VERSION}
downloadLink: '${FINAL_DOWNLOAD_URL}'
---

## Improvements

${IMPROVEMENTS}
EOF

# --- 4. EXPORT CONTRACT ---
echo "FINAL_VERSION=$VERSION" > release_info.env

echo "✅ Generated release-notes.mdx for version $VERSION"