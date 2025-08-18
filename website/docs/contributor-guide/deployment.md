---
title: Deployment
icon: material/cloud-upload
---

# Deploying the CYCODEV Documentation

This guide explains how to build and deploy the CYCODEV documentation website.

## Building the Documentation

Before deploying, you need to build the static site from your Markdown files:

```bash
mkdocs build
```

This command creates a `site/` directory containing the static HTML, CSS, and JavaScript files that make up the website.

## Understanding the Deployment Process

The CYCODEV documentation is deployed as a static website on Azure Storage. The deployment is handled by the `upload-website.sh` script in the `scripts/` directory.

### The Deployment Script

The `scripts/upload-website.sh` script automates the process of deploying the site to Azure. Here's what it does:

1. Optionally rebuilds the website using `mkdocs build`
2. Creates or verifies an Azure resource group
3. Creates or uses an existing Azure storage account
4. Enables static website hosting on the storage account
5. Uploads the contents of the `site/` directory to the storage account
6. Outputs the URL of the deployed website

## Prerequisites for Deployment

Before deploying, you need:

1. **Azure CLI**: Installed and configured on your machine
2. **Azure Account**: With appropriate permissions to create/modify storage accounts
3. **Login Status**: You must be logged in to Azure CLI (`az login`)

## Deployment Steps

To deploy the documentation:

### 1. Prepare Your Changes

Make sure all your changes are committed and tested locally using `mkdocs serve`.

### 2. Run the Deployment Script

```bash
cd /path/to/cycod/website
./scripts/upload-website.sh
```

#### Script Options

The script accepts several command-line options:

- `-r, --rebuild`: Force rebuild the website before uploading
- `-f, --force-new`: Force recreation of the storage account (deletes existing account)
- `-l, --location`: Specify Azure region (default: eastus)
- `-h, --help`: Display help message

Example with options:

```bash
./scripts/upload-website.sh --rebuild --location westus2
```

### 3. Verify the Deployment

After the script completes, it will display the URL of the deployed website. Open this URL in your browser to verify that your changes appear correctly.

## Testing Before Deployment

Before deploying to production, it's a good practice to:

1. Build the site locally with `mkdocs build`
2. Check the generated HTML in the `site/` directory
3. Verify that all links work and content displays correctly

You can preview the built site locally using a simple HTTP server:

```bash
cd site
python -m http.server 8000
```

Then open `http://localhost:8000` in your browser.

## Troubleshooting Deployment Issues

### Common Issues

1. **Authentication Errors**: Make sure you're logged in to Azure CLI with `az login`
2. **Permission Issues**: Ensure your Azure account has permissions to create/modify resources
3. **Name Conflicts**: Storage account names must be globally unique in Azure
4. **Missing Files**: Verify that the `site/` directory exists and contains your built website

### Checking Logs

The deployment script outputs information about each step. If there's an error, review the console output for clues about what went wrong.

## Next Steps

Now that you understand how to deploy the documentation:

- Check out additional [resources](resources.md) for learning more about MkDocs and documentation
- Explore contributing to other aspects of the CYCODEV project