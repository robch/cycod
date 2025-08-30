#!/usr/bin/env node

import { Command } from 'commander';
import { ConfigCommand } from '../commands/ConfigCommand';

const program = new Command();

program
  .name('cycodjs')
  .description('CYCODEV CLI - AI-powered command-line interface')
  .version('1.0.0');

program
  .command('config')
  .description('Manage configuration settings')
  .addCommand(ConfigCommand.createCommand());

program.parse(process.argv);