#!/bin/bash

# Script to deploy a static website to Azure
# This script uses a fixed storage account name "cycoddocs"
# instead of generating timestamped names

set -e  # Exit immediately if a command exits with a non-zero status

# Configuration
RESOURCE_GROUP="robch-cycod-website-rg"
STORAGE_ACCOUNT_NAME="cycoddocs"  # Fixed storage account name
LOCATION="eastus"  # Default location
WEBSITE_ROOT="."
SOURCE_DIR="$WEBSITE_ROOT/site"
WEBSITE_INDEX="index.html"
WEBSITE_ERROR="404.html"

# Function to display usage
usage() {
    echo "Usage: $0 [options]"
    echo "Options:"
    echo "  -h, --help        Display this help message"
    echo "  -r, --rebuild     Force rebuild the website with 'mkdocs build' before uploading"
    echo "  -f, --force-new   Force recreation of the storage account (WARNING: deletes existing account)"
    echo "  -l, --location    Specify Azure region (default: eastus)"
    exit 1
}

# Parse command line arguments
REBUILD=false
FORCE_NEW=false

while [[ $# -gt 0 ]]; do
    case $1 in
        -h|--help)
            usage
            ;;
        -r|--rebuild)
            REBUILD=true
            shift
            ;;
        -f|--force-new)
            FORCE_NEW=true
            shift
            ;;
        -l|--location)
            if [[ -z "$2" || "$2" == -* ]]; then
                echo "Error: Location argument requires a value."
                usage
            fi
            LOCATION="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            usage
            ;;
    esac
done

# Print banner
echo "====================================="
echo "Static Website Deployment to Azure"
echo "====================================="
echo "Current directory: $(pwd)"
echo "Source directory: $SOURCE_DIR"
echo "Using fixed storage account: $STORAGE_ACCOUNT_NAME"

# Check if user is logged in to Azure CLI
echo "Checking Azure CLI login status..."
SUBSCRIPTION=$(az account show --query "name" -o tsv 2>/dev/null || echo "")

if [ -z "$SUBSCRIPTION" ]; then
    echo "Error: Not logged into Azure CLI. Please run 'az login' first."
    exit 1
fi

echo "Logged in to subscription: $SUBSCRIPTION"

# Rebuild the website if requested
if [ "$REBUILD" = true ]; then
    echo "Rebuilding website with mkdocs..."
    # Change to the website root directory to run mkdocs
    cd $WEBSITE_ROOT
    mkdocs build
    if [ $? -ne 0 ]; then
        echo "Error: Failed to build website with mkdocs."
        cd - > /dev/null
        exit 1
    fi
    # Return to the scripts directory
    cd - > /dev/null
    echo "Website rebuilt successfully."
fi

# Check if the website files exist
if [ ! -d "$SOURCE_DIR" ]; then
    echo "Error: Source directory '$SOURCE_DIR' not found."
    echo "Run 'mkdocs build' in the website root directory to generate the website files first."
    exit 1
fi

# Check if index.html exists in the source directory
if [ ! -f "$SOURCE_DIR/$WEBSITE_INDEX" ]; then
    echo "Error: Index file '$SOURCE_DIR/$WEBSITE_INDEX' not found."
    echo "Make sure the website has been properly built."
    exit 1
fi

# Create resource group if it doesn't exist
echo "Checking resource group..."
GROUP_EXISTS=$(az group exists --name "$RESOURCE_GROUP")

if [ "$GROUP_EXISTS" = "false" ]; then
    echo "Creating resource group '$RESOURCE_GROUP'..."
    az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
    echo "Resource group created."
else
    echo "Resource group '$RESOURCE_GROUP' already exists."
fi

# Function to verify storage account exists and is accessible
verify_storage_account() {
    local account_name="$1"
    echo "Verifying storage account '$account_name'..."
    
    # Try to get the account properties as a test
    if az storage account show --name "$account_name" --resource-group "$RESOURCE_GROUP" --query "name" -o tsv &>/dev/null; then
        echo "Storage account '$account_name' exists and is accessible."
        return 0
    else
        echo "Storage account '$account_name' doesn't exist or isn't accessible."
        return 1
    fi
}

# Check if the storage account exists
create_new_account=false

if [ "$FORCE_NEW" = true ]; then
    echo "Force recreation of storage account requested."
    
    # Check if the account already exists
    if verify_storage_account "$STORAGE_ACCOUNT_NAME"; then
        echo "WARNING: Deleting existing storage account '$STORAGE_ACCOUNT_NAME'..."
        echo "This will permanently delete all content. You have 5 seconds to cancel (Ctrl+C)."
        sleep 5
        
        # Delete the existing account
        az storage account delete --name "$STORAGE_ACCOUNT_NAME" --resource-group "$RESOURCE_GROUP" --yes
        echo "Existing storage account deleted."
    fi
    
    create_new_account=true
else
    # Check if the account exists and is accessible
    if ! verify_storage_account "$STORAGE_ACCOUNT_NAME"; then
        echo "Storage account '$STORAGE_ACCOUNT_NAME' will be created."
        create_new_account=true
    else
        echo "Using existing storage account: $STORAGE_ACCOUNT_NAME"
    fi
fi

# Create storage account if needed
if [ "$create_new_account" = true ]; then
    echo "Creating storage account: $STORAGE_ACCOUNT_NAME"
    
    # Check if the name is available
    name_available=$(az storage account check-name --name "$STORAGE_ACCOUNT_NAME" --query "nameAvailable" -o tsv)
    
    if [ "$name_available" = "false" ]; then
        echo "Error: The storage account name '$STORAGE_ACCOUNT_NAME' is not available."
        echo "This could mean one of two things:"
        echo "1. The account exists in another subscription or resource group"
        echo "2. The name is already taken by someone else in Azure"
        echo "Please choose a different name and update the script."
        exit 1
    fi
    
    # Create the storage account
    az storage account create \
        --name "$STORAGE_ACCOUNT_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --location "$LOCATION" \
        --sku Standard_LRS \
        --kind StorageV2 \
        --https-only true

    if [ $? -ne 0 ]; then
        echo "Failed to create storage account. Please try a different name."
        exit 1
    fi

    echo "Storage account created successfully."
    
    # Verify the new account
    if ! verify_storage_account "$STORAGE_ACCOUNT_NAME"; then
        echo "Error: Could not access the newly created storage account."
        exit 1
    fi
fi

# Enable static website hosting
echo "Enabling static website hosting..."
az storage blob service-properties update \
    --account-name "$STORAGE_ACCOUNT_NAME" \
    --static-website \
    --index-document "$WEBSITE_INDEX" \
    --404-document "$WEBSITE_ERROR" \
    --auth-mode login

echo "Static website hosting enabled."

# Upload website files
echo "Uploading website files to storage..."

# Get storage account key for more reliable auth with the blob commands
echo "Getting storage account key..."
ACCOUNT_KEY=$(az storage account keys list --resource-group "$RESOURCE_GROUP" --account-name "$STORAGE_ACCOUNT_NAME" --query [0].value -o tsv)

if [ -z "$ACCOUNT_KEY" ]; then
    echo "Warning: Could not retrieve storage account key. Will try to use login auth mode."
    # Try with login auth
    az storage blob upload-batch \
        --account-name "$STORAGE_ACCOUNT_NAME" \
        --auth-mode login \
        --source "$SOURCE_DIR" \
        --destination '$web' \
        --overwrite
else
    # Upload with account key auth
    az storage blob upload-batch \
        --account-name "$STORAGE_ACCOUNT_NAME" \
        --account-key "$ACCOUNT_KEY" \
        --source "$SOURCE_DIR" \
        --destination '$web' \
        --overwrite
fi

echo "Website files uploaded successfully."

# Get the website URL
WEBSITE_URL=$(az storage account show \
    --name "$STORAGE_ACCOUNT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query "primaryEndpoints.web" \
    --output tsv)

echo "====================================="
echo "Deployment complete!"
echo "Website URL: $WEBSITE_URL"
echo "====================================="

# Validate the deployment
echo "Validating deployment with curl..."
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$WEBSITE_URL")
if [ "$HTTP_STATUS" = "200" ]; then
    echo "Website is accessible (HTTP 200 OK)"
    echo "Fetching website content..."
    curl -s "$WEBSITE_URL" | head -n 10
    echo "..."
else
    echo "Warning: Website returned HTTP status $HTTP_STATUS"
fi

# Display permanent URL information
echo ""
echo "====================================="
echo "PERMANENT WEBSITE INFORMATION"
echo "====================================="
echo "Your website is now deployed to a fixed storage account."
echo "The URL will remain stable for all future deployments:"
echo "$WEBSITE_URL"
echo ""
echo "To update the website, simply run this script again."
echo "====================================="

echo "Done!"