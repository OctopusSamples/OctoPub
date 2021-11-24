/**
 * Represents the configuration in the config.json file, which is processed by Octopus for each deployment
 * and environment.
 */
export interface DynamicConfig {
    settings: {
        basename: string;
        productEndpoint: string;
        auditEndpoint: string;
        title: string;
        editorFormat: string;
        google: {
            tag: string;
        }
    },
    useDefaultTheme?: boolean;
    apiKey: string | null;
    setApiKey: (key: string) => void;
    partition: string | null;
    setPartition: (id: string | null) => void;
    setAllBookId: (id: string | null) => void;
    allBookId: string | null;
}