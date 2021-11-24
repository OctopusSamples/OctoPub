import clsx from "clsx";
import {
    AppBar,
    createStyles,
    IconButton,
    Link,
    makeStyles,
    Theme,
    Toolbar,
    Tooltip,
    Typography,
} from "@material-ui/core";
import Brightness7Icon from "@material-ui/icons/Brightness7";
import Brightness3Icon from "@material-ui/icons/Brightness3";
import {FC, useContext} from "react";
import {AppContext} from "../App";
import {useHistory} from "react-router-dom";
import {AddCircleOutline, Delete, Edit, History, SettingsApplications} from "@material-ui/icons";
import {CommonProps} from "../model/RouteItem.model";

// define css-in-js
const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        appBar: {
            zIndex: theme.zIndex.drawer + 1,
            transition: theme.transitions.create(["width", "margin"], {
                easing: theme.transitions.easing.sharp,
                duration: theme.transitions.duration.leavingScreen,
            }),
        },
        toolbar: {
            flex: 1,
            display: "flex",
            flexDirection: "row",
            alignItems: "center",
        },
        title: {
            flex: 1,
            display: "flex",
            flexDirection: "row",
            alignItems: "center"
        },
        menuButton: {
            marginRight: 36,
        },
        hide: {
            display: "none",
        },
        heading: {
            color: "white",
            cursor: "pointer"
        }
    })
);

// define interface to represent component props
interface HeaderProps extends CommonProps {
    toggleTheme: () => void;
    useDefaultTheme: boolean;
}

const Header: FC<HeaderProps> = ({
                                     toggleTheme,
                                     useDefaultTheme
                                 }: HeaderProps) => {
    const classes = useStyles();
    const context = useContext(AppContext);
    const history = useHistory();
    return (
        <AppBar
            position="relative"
            elevation={0}
            className={clsx(classes.appBar)}
        >
            <Toolbar className={classes.toolbar}>
                <div className={classes.title}>
                    <Link className={classes.heading} onClick={() => {
                        history.push('/index.html');
                    }}>
                        <Typography variant="h6" noWrap>
                            {context.settings.title}
                        </Typography>
                    </Link>
                </div>
                {context.allBookId && context.apiKey &&
                <IconButton onClick={() => history.push('/deleteBook/' + context.allBookId)}>
                    <Tooltip title={"Delete"} placement={"bottom"}>
                        <Delete/>
                    </Tooltip>
                </IconButton>
                }
                {context.allBookId && context.apiKey &&
                <IconButton onClick={() => history.push('/updateBook/' + context.allBookId)}>
                    <Tooltip title={"Update"} placement={"bottom"}>
                        <Edit/>
                    </Tooltip>
                </IconButton>
                }
                {context.apiKey &&
                <IconButton onClick={() => history.push('/addBook')}>
                    <Tooltip title={"Add Book"} placement={"bottom"}>
                        <AddCircleOutline/>
                    </Tooltip>
                </IconButton>
                }
                <IconButton onClick={() => history.push('/audits')}>
                    <Tooltip title={"Audits"} placement={"bottom"}>
                        <History/>
                    </Tooltip>
                </IconButton>
                <IconButton onClick={() => history.push('/settings')}>
                    <Tooltip title={"Settings"} placement={"bottom"}>
                        <SettingsApplications/>
                    </Tooltip>
                </IconButton>
                <IconButton onClick={toggleTheme}>
                    {useDefaultTheme ? (
                        <Tooltip title="Switch to dark mode" placement="bottom">
                            <Brightness3Icon/>
                        </Tooltip>
                    ) : (
                        <Tooltip title="Switch to light mode" placement="bottom">
                            <Brightness7Icon/>
                        </Tooltip>
                    )}
                </IconButton>
            </Toolbar>
        </AppBar>
    );
};

export default Header;