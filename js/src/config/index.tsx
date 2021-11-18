// icons
import HomeIcon from '@material-ui/icons/Home';

// components
import Home from '../pages/Home';

// interface
import RouteItem, {CommonProps} from '../model/RouteItem.model';

// define app routes
export const routes: Array<RouteItem> = [
    {
        key: "router-home",
        title: "Home",
        tooltip: "Home",
        path: "/",
        enabled: true,
        component: (props: CommonProps) => () => <Home {...props}/>,
        icon: HomeIcon,
        appendDivider: true
    },
    {
        key: "router-home-index",
        title: "Home",
        tooltip: "Home",
        path: "/index.html",
        enabled: true,
        component: (props: CommonProps) => () => <Home {...props}/>,
        icon: HomeIcon,
        appendDivider: true
    }
]