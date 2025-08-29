"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ConfigFileScope = void 0;
/**
 * Defines the scope of configuration settings.
 */
var ConfigFileScope;
(function (ConfigFileScope) {
    /**
     * Global settings (system-wide).
     */
    ConfigFileScope["Global"] = "global";
    /**
     * User settings (user-specific).
     */
    ConfigFileScope["User"] = "user";
    /**
     * Local settings (project-specific).
     */
    ConfigFileScope["Local"] = "local";
    /**
     * Config file settings (from explicitly loaded configuration file paths).
     */
    ConfigFileScope["FileName"] = "filename";
    /**
     * All scopes (used for display/querying across scopes).
     */
    ConfigFileScope["Any"] = "any";
})(ConfigFileScope || (exports.ConfigFileScope = ConfigFileScope = {}));
//# sourceMappingURL=ConfigFileScope.js.map