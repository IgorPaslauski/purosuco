const vscode = require('vscode');
const path = require('path');
const fs = require('fs');
const {
  LanguageClient,
  TransportKind
} = require('vscode-languageclient/node');

let client;

function workspaceRoot() {
  const folder = vscode.workspace.workspaceFolders?.[0];
  return folder?.uri.fsPath;
}

function activeSuco() {
  const editor = vscode.window.activeTextEditor;
  if (!editor || editor.document.languageId !== 'purosuco') {
    vscode.window.showErrorMessage('Abre um .suco primeiro, amostradinho.');
    return null;
  }
  return editor.document;
}

function terminalFor(document, command) {
  const root = workspaceRoot();
  if (!root) {
    vscode.window.showErrorMessage('Abre a pasta do projeto PuroSuco primeiro.');
    return;
  }

  const project = path.join(root, 'src', 'PuroSuco.Cli', 'PuroSuco.Cli.csproj');
  const terminal = vscode.window.createTerminal('🥤 PuroSuco');
  terminal.show();
  terminal.sendText(`dotnet run --project "${project}" -- ${command} "${document.uri.fsPath}"`);
}

async function activate(context) {
  const root = workspaceRoot();

  if (root) {
    const serverProject = path.join(root, 'src', 'PuroSuco.LanguageServer', 'PuroSuco.LanguageServer.csproj');

    if (fs.existsSync(serverProject)) {
      const serverOptions = {
        run: {
          command: 'dotnet',
          args: ['run', '--project', serverProject],
          transport: TransportKind.stdio
        },
        debug: {
          command: 'dotnet',
          args: ['run', '--project', serverProject],
          transport: TransportKind.stdio
        }
      };

      const clientOptions = {
        documentSelector: [{ scheme: 'file', language: 'purosuco' }],
        synchronize: {
          fileEvents: vscode.workspace.createFileSystemWatcher('**/*.suco')
        }
      };

      client = new LanguageClient(
        'purosucoLanguageServer',
        'PuroSuco Language Server',
        serverOptions,
        clientOptions
      );

      await client.start();
    } else {
      vscode.window.showWarningMessage('PuroSuco Language Server não encontrado neste workspace.');
    }
  }

  context.subscriptions.push(
    vscode.commands.registerCommand('purosuco.run', async () => {
      const doc = activeSuco();
      if (!doc) return;
      await doc.save();
      terminalFor(doc, 'roda');
    }),
    vscode.commands.registerCommand('purosuco.transpile', async () => {
      const doc = activeSuco();
      if (!doc) return;
      await doc.save();
      terminalFor(doc, 'traduz');
    }),
    vscode.commands.registerCommand('purosuco.keywords', async () => {
      const content = [
        'AMOSTRADINHO → public',
        'NA_MIÚDA → private',
        'TROPA → class',
        'TA_CERTO_ISSO → if',
        'NAO_TA_NAO → else',
        'RECEBA → =',
        'CONFIA → true',
        'CONFIA_NAO → false',
        'TEM_NADA_AI → null',
        'ENQUANTO_TANKAR → while',
        'BORA_BILL → for',
        'VAI_DAR_BOM → try',
        'METEU_ESSA → catch',
        'TOMA → return',
        'MANDA_AI → Console.WriteLine'
      ].join('\n');

      const doc = await vscode.workspace.openTextDocument({
        content,
        language: 'plaintext'
      });

      await vscode.window.showTextDocument(doc, { preview: true });
    })
  );
}

async function deactivate() {
  if (client) {
    await client.stop();
  }
}

module.exports = { activate, deactivate };
