import {ComponentType, FC} from "react";

export interface CommonProps {
    apiKey: string | null;
    setAllBookId: (id: string | null) => void;
    allBookId: string | null;
}

// RouteItem is an interface for defining the application routes and navigation menu items
interface RouteItem {
    key: string;
    title: string;
    tooltip?: string;
    path?: string;
    component?: (props: CommonProps) => FC<CommonProps>;
    enabled: boolean;
    icon?: ComponentType;
    subRoutes?: Array<RouteItem>;
    appendDivider?: boolean;
}

export default RouteItem;